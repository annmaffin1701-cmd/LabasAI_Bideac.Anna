using UnityEngine;

public class Enemy2 : EnemyFSMBase
{
    // 360 SCAN
    [Header("360 Scan")]

    [Tooltip("Радиус обнаружения игрока")]
    [SerializeField]
    private float scanRadius = 5f;

    [Tooltip("Как часто выполнять проверку обнаружения")]
    [SerializeField]
    private float scanInterval = 0.15f;
    // TRACKING
    [Header("Tracking")]

    [Tooltip("Максимальная дистанция, на которой враг продолжает преследование")]
    [SerializeField]
    private float trackingDistance = 14f;
    // TELEPORT
    [Header("Teleport")]

    [Tooltip("Перезарядка телепорта")]
    [SerializeField]
    private float teleportCooldown = 5f;

    [Tooltip("Если игрок дальше этого расстояния, враг может телепортироваться")]
    [SerializeField]
    private float teleportTriggerDistance = 7f;

    [Tooltip("Максимальное расстояние одного телепорта. 0 = без ограничения")]
    [SerializeField]
    private float teleportMaxDistance = 20f;

    [Space]

    [Tooltip("Минимальное расстояние появления от игрока")]
    [SerializeField]
    private float teleportMinPlayerDistance = 1.8f;

    [Tooltip("Максимальное расстояние появления от игрока")]
    [SerializeField]
    private float teleportMaxPlayerDistance = 3f;

    [Space]

    [Tooltip("Радиус проверки свободного места")]
    [SerializeField]
    private float teleportSafeRadius = 0.3f;

    [Tooltip("Количество попыток найти свободное место")]
    [SerializeField]
    private int teleportAttempts = 40;

    [Tooltip("Необязательная область, внутри которой разрешён телепорт")]
    [SerializeField]
    private Collider2D teleportArea;
    // DEBUG
    [Header("Debug")]

    [SerializeField]
    private bool showTeleportDebug = true;
    // PRIVATE
    private float scanTimer;
    private float teleportTimer;
    // START
    protected override void Start()
    {
        base.Start();

        scanTimer = 0f;

        // Телепорт доступен сразу после начала игры.
        // Если хочешь задержку первого телепорта:
        // teleportTimer = teleportCooldown;
        teleportTimer = 0f;
    }
    // SPECIAL TICK
    protected override void TickSpecial(float deltaTime)
    {
        if (scanTimer > 0f)
        {
            scanTimer -= deltaTime;
        }

        if (teleportTimer > 0f)
        {
            teleportTimer -= deltaTime;
        }
    }
    // PLAYER DETECTION
    protected override bool DetectPlayer()
    {
        if (player == null)
            return false;


        // Не сканируем каждый кадр
        if (scanTimer > 0f)
            return false;


        scanTimer = scanInterval;


        float distance = GetPlayerDistance();


        // Игрок слишком далеко
        if (distance > scanRadius)
            return false;


        // Между врагом и игроком стена
        if (!HasLineOfSight())
            return false;


        return true;
    }
    // TRACK PLAYER
    protected override bool CanStillTrackPlayer()
    {
        if (player == null)
            return false;


        float distance = GetPlayerDistance();


        // Слишком далеко
        if (distance > trackingDistance)
            return false;


        // Потеряли визуальный контакт
        if (!HasLineOfSight())
            return false;


        return true;
    }
    // SHOULD TELEPORT
    protected override bool ShouldTeleport()
    {
        if (player == null)
            return false;


        // Телепорт ещё на cooldown
        if (teleportTimer > 0f)
            return false;


        float distance = GetPlayerDistance();


        if (distance < teleportTriggerDistance)
            return false;


        if (showTeleportDebug)
        {
            Debug.Log(
                $"{name}: TELEPORT REQUEST | " +
                $"Distance: {distance:F2}"
            );
        }


        return true;
    }
    // PERFORM TELEPORT
    protected override void PerformTeleport()
    {
        if (player == null)
        {
            teleportTimer = teleportCooldown;
            return;
        }


        Vector2 currentPosition = rb.position;
        Vector2 playerPosition = player.position;


        Vector2 bestPosition = currentPosition;

        bool foundPosition = false;

        float bestScore = Mathf.Infinity;


        // Debug counters
        int rejectedByDistance = 0;
        int rejectedByArea = 0;
        int rejectedByObstacle = 0;
        // FIND POSITION

        for (int i = 0; i < teleportAttempts; i++)
        {
            // Случайное направление вокруг игрока
            Vector2 direction =
                Random.insideUnitCircle;


            // Иногда Random.insideUnitCircle
            // может дать почти нулевой Vector2
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }


            direction.Normalize();


            // Случайная дистанция от Player
            float distanceFromPlayer =
                Random.Range(
                    teleportMinPlayerDistance,
                    teleportMaxPlayerDistance
                );


            // Потенциальная точка
            Vector2 candidate =
                playerPosition +
                direction * distanceFromPlayer;
            // CHECK MAX TELEPORT DISTANCE

            if (teleportMaxDistance > 0f)
            {
                float teleportDistance =
                    Vector2.Distance(
                        currentPosition,
                        candidate
                    );


                if (teleportDistance > teleportMaxDistance)
                {
                    rejectedByDistance++;

                    continue;
                }
            }
            // CHECK TELEPORT AREA
            if (teleportArea != null)
            {
                if (!teleportArea.OverlapPoint(candidate))
                {
                    rejectedByArea++;
                    continue;
                }
            }
            // CHECK OBSTACLE

            Collider2D obstacle =
                Physics2D.OverlapCircle(
                    candidate,
                    teleportSafeRadius,
                    obstacleMask
                );


            if (obstacle != null)
            {
                rejectedByObstacle++;


                if (showTeleportDebug)
                {
                    Debug.Log(
                        $"{name}: teleport point blocked by " +
                        $"{obstacle.name} | Layer: " +
                        $"{LayerMask.LayerToName(obstacle.gameObject.layer)}"
                    );
                }


                continue;
            }
            // POSITION IS VALID
            float actualDistanceToPlayer =
                Vector2.Distance(
                    candidate,
                    playerPosition
                );


            // Предпочитаем примерно среднюю дистанцию
            float desiredDistance =
                (
                    teleportMinPlayerDistance +
                    teleportMaxPlayerDistance
                ) * 0.5f;


            float score =
                Mathf.Abs(
                    actualDistanceToPlayer -
                    desiredDistance
                );


            if (score < bestScore)
            {
                bestScore = score;

                bestPosition = candidate;

                foundPosition = true;
            }
        }
        // APPLY TELEPORT
        if (foundPosition)
        {
            Vector2 oldPosition = rb.position;


            rb.position = bestPosition;


            Physics2D.SyncTransforms();


            if (showTeleportDebug)
            {
                Debug.Log(
                    $"{name}: TELEPORT SUCCESS\n" +
                    $"FROM: {oldPosition}\n" +
                    $"TO: {bestPosition}\n" +
                    $"PLAYER: {playerPosition}\n" +
                    $"DISTANCE TO PLAYER: " +
                    $"{Vector2.Distance(bestPosition, playerPosition):F2}"
                );
            }
        }
        // FAILED
        else
        {
            Debug.LogWarning(
                $"{name}: TELEPORT FAILED\n" +
                $"Player distance: {GetPlayerDistance():F2}\n" +
                $"Attempts: {teleportAttempts}\n" +
                $"Rejected Distance: {rejectedByDistance}\n" +
                $"Rejected Area: {rejectedByArea}\n" +
                $"Rejected Obstacles: {rejectedByObstacle}"
            );
        }


        // Запускаем cooldown
        teleportTimer = teleportCooldown;
    }
    // DEBUG GIZMOS
    protected override void DrawExtraGizmos()
    {
        // DETECTION
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            scanRadius
        );
        // TRACKING DISTANCE
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            trackingDistance
        );
        // TELEPORT TRIGGER
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            teleportTriggerDistance
        );
        // MAX TELEPORT DISTANCE
        if (teleportMaxDistance > 0f)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                teleportMaxDistance
            );
        }
        // PLAYER TELEPORT AREA
        if (player != null)
        {
            // Минимальная дистанция появления
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                player.position,
                teleportMinPlayerDistance
            );


            // Максимальная дистанция появления
            Gizmos.color = new Color(
                0f,
                1f,
                0f,
                0.5f
            );

            Gizmos.DrawWireSphere(
                player.position,
                teleportMaxPlayerDistance
            );
        }
    }
}