using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Patrol")]
    [SerializeField] private float patrolDistanceMin = 2f;
    [SerializeField] private float patrolDistanceMax = 5f;
    [SerializeField] private float destinationTolerance = 0.15f;
    [SerializeField] private float idleTime = 1.5f;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float viewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Attack")]
    [SerializeField] private float attackDistance = 1.2f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Search")]
    [SerializeField] private float searchTime = 4f;
    [SerializeField] private float searchSpeed = 1.5f;

    private Rigidbody2D rb;

    private State state;

    private Vector2 startingPosition;
    private Vector2 patrolPosition;
    private Vector2 lastKnownPlayerPosition;

    private float stateTimer;
    private float attackTimer;

    private bool playerDead;
    private bool enemyDead;

    public event Action OnAttack;

    private enum State
    {
        Start,          // S0
        Idle,           // I
        Patrol,         // P
        Chase,          // C
        Attack,         // A
        Search,         // S
        PlayerDeath,    // D
        EnemyDeath      // DE
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        state = State.Start;
    }

    private void Start()
    {
        // Если игрок не назначен вручную
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        ChangeState(State.Idle);
    }

    private void Update()
    {
        if (enemyDead)
        {
            if (state != State.EnemyDeath)
                ChangeState(State.EnemyDeath);

            return;
        }

        if (playerDead)
        {
            if (state != State.PlayerDeath)
                ChangeState(State.PlayerDeath);

            return;
        }

        attackTimer -= Time.deltaTime;

        UpdateState();
        UpdateAnimation();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (enemyDead || playerDead)
            return;

        switch (state)
        {
            case State.Patrol:
                MoveTo(patrolPosition, patrolSpeed);
                break;

            case State.Chase:
                if (player != null)
                    MoveTo(player.position, chaseSpeed);
                break;

            case State.Search:
                MoveTo(lastKnownPlayerPosition, searchSpeed);
                break;
        }
    }

    // STATE MACHINE


    private void UpdateState()
    {
        switch (state)
        {
            case State.Idle:
                IdleState();
                break;

            case State.Patrol:
                PatrolState();
                break;

            case State.Chase:
                ChaseState();
                break;

            case State.Attack:
                AttackState();
                break;

            case State.Search:
                SearchState();
                break;

            case State.PlayerDeath:
                PlayerDeathState();
                break;

            case State.EnemyDeath:
                EnemyDeathState();
                break;
        }
    }

    // IDLE


    private void IdleState()
    {
        // I -> close -> A
        if (IsPlayerClose())
        {
            ChangeState(State.Attack);
            return;
        }

        // I -> see -> C
        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        stateTimer -= Time.deltaTime;

        // I -> init -> P
        if (stateTimer <= 0f)
        {
            ChangeState(State.Patrol);
        }
    }

    // PATROL


    private void PatrolState()
    {
        // P -> close -> A
        if (IsPlayerClose())
        {
            ChangeState(State.Attack);
            return;
        }

        // P -> see -> C
        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        float distance = Vector2.Distance(
            rb.position,
            patrolPosition
        );

        // Дошёл до случайной точки
        if (distance <= destinationTolerance)
        {
            ChangeState(State.Idle);
        }
    }


    // CHASE


    private void ChaseState()
    {
        if (player == null)
            return;

        // C -> close -> A
        if (IsPlayerClose())
        {
            ChangeState(State.Attack);
            return;
        }

        // C -> lost -> S
        if (!CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;

            ChangeState(State.Search);
            return;
        }

        lastKnownPlayerPosition = player.position;
    }


    // ATTACK


    private void AttackState()
    {
        if (player == null)
            return;

        // A -> lost -> S
        if (!CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;

            ChangeState(State.Search);
            return;
        }

        // Игрок виден, но уже далеко
        // A -> see -> C
        if (!IsPlayerClose())
        {
            ChangeState(State.Chase);
            return;
        }

        // A -> aim -> A
        if (attackTimer <= 0f)
        {
            AttackPlayer();

            attackTimer = attackCooldown;
        }
    }

    // SEARCH

    private void SearchState()
    {
        // S -> close -> A
        if (IsPlayerClose())
        {
            ChangeState(State.Attack);
            return;
        }

        // S -> see -> C
        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        stateTimer -= Time.deltaTime;

        // S -> timeout -> I
        if (stateTimer <= 0f)
        {
            ChangeState(State.Idle);
            return;
        }

        float distance = Vector2.Distance(
            rb.position,
            lastKnownPlayerPosition
        );

        // Дошли до последней известной позиции.
        if (distance <= destinationTolerance)
        {
            // Остаёмся в Search, пока не закончится searchTime.
        }
    }
    // PLAYER DEATH

    private void PlayerDeathState()
    {

        // Здесь враг ничего не делает.
        // После рестарта уровня можно вызвать ResetEnemy().
    }
    // ENEMY DEATH

    private void EnemyDeathState()
    {
        rb.linearVelocity = Vector2.zero;
    }
    // CHANGE STATE

    private void ChangeState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.Idle:

                stateTimer = idleTime;

                break;

            case State.Patrol:

                StartPatrol();

                break;

            case State.Chase:

                if (player != null)
                    lastKnownPlayerPosition = player.position;

                break;

            case State.Attack:

                rb.linearVelocity = Vector2.zero;

                break;

            case State.Search:

                stateTimer = searchTime;

                break;

            case State.PlayerDeath:

                rb.linearVelocity = Vector2.zero;

                break;

            case State.EnemyDeath:

                rb.linearVelocity = Vector2.zero;

                break;
        }
    }
    // PATROL
    private void StartPatrol()
    {
        startingPosition = rb.position;

        patrolPosition = Utils.GetRandomPosition2D(
            startingPosition,
            patrolDistanceMin,
            patrolDistanceMax
        );
    }
    // MOVEMENT

    private void MoveTo(Vector2 target, float speed)
    {
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    // VISION
    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 directionToPlayer =
            (Vector2)player.position - rb.position;

        float distance = directionToPlayer.magnitude;

        // Игрок слишком далеко
        if (distance > viewDistance)
            return false;

        // Направление, куда смотрит враг
        Vector2 facingDirection =
            spriteRenderer != null && spriteRenderer.flipX
            ? Vector2.left
            : Vector2.right;

        float angle = Vector2.Angle(
            facingDirection,
            directionToPlayer.normalized
        );

        if (angle > viewAngle * 0.5f)
            return false;

        // Проверяем стену между врагом и игроком
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            directionToPlayer.normalized,
            distance,
            obstacleMask
        );

        if (hit.collider != null)
            return false;

        return true;
    }

    private bool IsPlayerClose()
    {
        if (player == null)
            return false;

        float distance = Vector2.Distance(
            rb.position,
            player.position
        );

        return distance <= attackDistance;
    }

    // ATTACK

    private void AttackPlayer()
    {
        Debug.Log("Enemy Attack");

        // Здесь можно подключить оружие / урон.
        OnAttack?.Invoke();
    }
    // FACING
    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null)
            return;

        Vector2 targetPosition;

        switch (state)
        {
            case State.Chase:
            case State.Attack:

                if (player == null)
                    return;

                targetPosition = player.position;

                break;

            case State.Patrol:

                targetPosition = patrolPosition;

                break;

            case State.Search:

                targetPosition = lastKnownPlayerPosition;

                break;

            default:
                return;
        }

        spriteRenderer.flipX =
            targetPosition.x < transform.position.x;
    }

    // ANIMATION

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isMoving =
            state == State.Patrol ||
            state == State.Chase ||
            state == State.Search;

        animator.SetBool("IsMoving", isMoving);

        animator.SetBool(
            "IsAttacking",
            state == State.Attack
        );

        animator.SetBool(
            "IsDead",
            state == State.EnemyDeath
        );
    }

    // EXTERNAL EVENTS

    public void NotifyPlayerDead()
    {
        playerDead = true;
    }

    public void Die()
    {
        enemyDead = true;
    }

    public void ResetEnemy()
    {
        playerDead = false;
        enemyDead = false;

        ChangeState(State.Idle);
    }
    // DEBUG

    private void OnDrawGizmosSelected()
    {
        // Радиус зрения
        Gizmos.DrawWireSphere(
            transform.position,
            viewDistance
        );

        // Радиус атаки
        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance
        );
    }
}