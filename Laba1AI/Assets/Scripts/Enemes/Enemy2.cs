using UnityEngine;

public class Enemy2 : EnemyFSMBase
{
    [Header("360 Scan")]
    [SerializeField] private float scanRadius = 1f;
    [SerializeField] private float scanInterval = 0.5f;

    [Header("Tracking")]
    [SerializeField] private float trackingDistance = 8f;

    [Header("Teleport")]
    [SerializeField] private float teleportCooldown = 5f;

    [SerializeField] private float teleportTriggerDistance = 4f;

    [SerializeField] private float teleportMaxDistance = 6f;

    [SerializeField] private float teleportMinPlayerDistance = 1.5f;

    [SerializeField] private float teleportMaxPlayerDistance = 2.5f;

    [SerializeField] private float teleportSafeRadius = 0.35f;

    [Tooltip("Optional trigger/collider that defines valid teleport area")]
    [SerializeField] private Collider2D teleportArea;

    private float scanTimer;
    private float teleportTimer;


    protected override void Start()
    {
        base.Start();

        scanTimer = 0f;
        teleportTimer = teleportCooldown;
    }


    protected override void TickSpecial(
        float deltaTime
    )
    {
        if (scanTimer > 0f)
            scanTimer -= deltaTime;

        if (teleportTimer > 0f)
            teleportTimer -= deltaTime;
    }


    // =====================================================
    // 360 SCAN
    // =====================================================

    protected override bool DetectPlayer()
    {
        if (player == null)
            return false;

        if (scanTimer > 0f)
            return false;

        scanTimer = scanInterval;

        float distance =
            GetPlayerDistance();

        if (distance > scanRadius)
            return false;

        if (!HasLineOfSight())
            return false;

        return true;
    }


    // После обнаружения враг может некоторое время
    // нормально отслеживать игрока
    protected override bool CanStillTrackPlayer()
    {
        if (player == null)
            return false;

        if (GetPlayerDistance() > trackingDistance)
            return false;

        return HasLineOfSight();
    }


    // =====================================================
    // TELEPORT
    // =====================================================

    protected override bool ShouldTeleport()
    {
        if (player == null)
            return false;

        if (teleportTimer > 0f)
            return false;

        return GetPlayerDistance() >=
               teleportTriggerDistance;
    }


    protected override void PerformTeleport()
    {
        if (player == null)
            return;

        Vector2 currentPosition =
            rb.position;

        Vector2 bestPosition =
            currentPosition;

        bool foundPosition = false;

        float bestDistanceToPlayer =
            Mathf.Infinity;


        for (int i = 0; i < 20; i++)
        {
            Vector2 direction =
                Random.insideUnitCircle.normalized;

            float playerDistance =
                Random.Range(
                    teleportMinPlayerDistance,
                    teleportMaxPlayerDistance
                );

            Vector2 candidate =
                (Vector2)player.position +
                direction *
                playerDistance;


            // Не дальше максимального teleport distance
            float teleportDistance =
                Vector2.Distance(
                    currentPosition,
                    candidate
                );

            if (teleportDistance >
                teleportMaxDistance)
            {
                continue;
            }


            // Если задана игровая область
            if (teleportArea != null)
            {
                if (!teleportArea.OverlapPoint(
                        candidate))
                {
                    continue;
                }
            }


            // Не внутри стены
            Collider2D obstacle =
                Physics2D.OverlapCircle(
                    candidate,
                    teleportSafeRadius,
                    obstacleMask
                );

            if (obstacle != null)
                continue;


            float distanceToPlayer =
                Vector2.Distance(
                    candidate,
                    player.position
                );


            if (distanceToPlayer <
                bestDistanceToPlayer)
            {
                bestDistanceToPlayer =
                    distanceToPlayer;

                bestPosition =
                    candidate;

                foundPosition = true;
            }
        }


        if (foundPosition)
        {
            rb.position =
                bestPosition;

            Physics2D.SyncTransforms();

            Debug.Log(
                $"{name} TELEPORT"
            );
        }


        teleportTimer =
            teleportCooldown;
    }


    // =====================================================
    // DEBUG
    // =====================================================

    protected override void DrawExtraGizmos()
    {
        // Scan 360
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            scanRadius
        );


        // Maximum teleport distance
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            teleportMaxDistance
        );
    }
}