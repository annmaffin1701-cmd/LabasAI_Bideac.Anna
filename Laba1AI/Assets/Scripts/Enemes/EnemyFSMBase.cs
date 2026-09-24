using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyFSMBase : MonoBehaviour
{
    protected enum State
    {
        Start,
        Idle,
        Patrol,
        Chase,
        Attack,
        Search,
        Teleport,
        PlayerDead,
        Dead
    }

    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] protected float patrolSpeed = 1.5f;
    [SerializeField] protected float chaseSpeed = 3f;
    [SerializeField] protected float searchSpeed = 1.5f;

    [Header("Idle")]
    [SerializeField] private float idleTime = 1.5f;

    [Header("Patrol")]
    [SerializeField] protected float patrolDistanceMin = 2f;
    [SerializeField] protected float patrolDistanceMax = 5f;
    [SerializeField] protected float destinationTolerance = 0.15f;

    [Header("Attack")]
    [SerializeField] protected float attackDistance = 1.2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;

    [Header("Search")]
    [SerializeField] private float searchTime = 4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Collision")]
    [SerializeField] protected LayerMask obstacleMask;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    protected Rigidbody2D rb;

    protected Vector2 patrolPosition;
    protected Vector2 lastKnownPlayerPosition;
    protected Vector2 lastMoveDirection = Vector2.right;

    private State currentState = State.Start;

    private PlayerHealth playerHealth;

    private float stateTimer;
    private float attackTimer;

    private int currentHealth;

    private Color originalColor;

    public string CurrentStateName => currentState.ToString();


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        currentHealth = maxHealth;
    }


    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        ChangeState(State.Idle);
    }


    protected virtual void Update()
    {
        if (currentState == State.Dead)
            return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            if (currentState != State.PlayerDead)
                ChangeState(State.PlayerDead);

            return;
        }

        TickSpecial(Time.deltaTime);

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;

            case State.Patrol:
                UpdatePatrol();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Search:
                UpdateSearch();
                break;

            case State.Teleport:
                UpdateTeleport();
                break;

            case State.PlayerDead:
                break;
        }
    }


    protected virtual void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Patrol:

                MoveTo(
                    patrolPosition,
                    patrolSpeed
                );

                break;

            case State.Chase:

                if (player != null)
                {
                    MoveTo(
                        player.position,
                        chaseSpeed
                    );
                }

                break;

            case State.Search:

                MoveTo(
                    lastKnownPlayerPosition,
                    searchSpeed
                );

                break;
        }
    }
    // IDLE
    private void UpdateIdle()
    {
        // I to close to A
        if (CanAttackPlayer())
        {
            ChangeState(State.Attack);
            return;
        }

        // I to see to C
        if (DetectPlayer())
        {
            RememberPlayerPosition();

            ChangeState(State.Chase);
            return;
        }

        stateTimer -= Time.deltaTime;

        // I to init to P
        if (stateTimer <= 0f)
        {
            ChangeState(State.Patrol);
        }
    }
    // PATROL
    private void UpdatePatrol()
    {
        // P to close to A
        if (CanAttackPlayer())
        {
            ChangeState(State.Attack);
            return;
        }

        // P to see to C
        if (DetectPlayer())
        {
            RememberPlayerPosition();

            ChangeState(State.Chase);
            return;
        }

        float distance =
            Vector2.Distance(
                rb.position,
                patrolPosition
            );

        if (distance <= destinationTolerance)
        {
            ChangeState(State.Idle);
        }
    }
    // CHASE
    private void UpdateChase()
    {
        if (player == null)
            return;

        // C to close to A
        if (CanAttackPlayer())
        {
            ChangeState(State.Attack);
            return;
        }

        // C to lost to S
        if (!CanStillTrackPlayer())
        {
            ChangeState(State.Search);
            return;
        }

        RememberPlayerPosition();

        // Только для телепорта
        // C to activate to T
        if (ShouldTeleport())
        {
            ChangeState(State.Teleport);
        }
    }
    // ATTACK
    private void UpdateAttack()
    {
        if (player == null)
            return;

        // A to lost to S
        if (!CanStillTrackPlayer())
        {
            ChangeState(State.Search);
            return;
        }

        // A to see to C
        if (!CanAttackPlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        // A to aim to A
        if (attackTimer <= 0f)
        {
            AttackPlayer();

            attackTimer = attackCooldown;
        }
    }
    // SEARCH
    private void UpdateSearch()
    {
        // S to close to A
        if (CanAttackPlayer())
        {
            ChangeState(State.Attack);
            return;
        }

        // S to see to C
        if (DetectPlayer())
        {
            RememberPlayerPosition();

            ChangeState(State.Chase);
            return;
        }

        stateTimer -= Time.deltaTime;

        // S to timeout to P
        if (stateTimer <= 0f)
        {
            ChangeState(State.Patrol);
        }
    }
    // TELEPORT
    private void UpdateTeleport()
    {
        PerformTeleport();

        if (CanStillTrackPlayer())
        {
            RememberPlayerPosition();

            ChangeState(State.Chase);
        }
        else
        {
            ChangeState(State.Search);
        }
    }
    // STATES
    protected void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case State.Idle:

                stateTimer = idleTime;
                break;

            case State.Patrol:

                CreatePatrolPosition();
                break;

            case State.Search:

                stateTimer = searchTime;
                break;

            case State.Attack:

                StopMovement();
                break;

            case State.Teleport:

                StopMovement();
                break;

            case State.PlayerDead:

                StopMovement();
                break;

            case State.Dead:

                StopMovement();
                break;
        }

        if (showDebug)
        {
            Debug.Log(
                $"{gameObject.name} -> {newState}"
            );
        }
    }
    // PATROL
    private void CreatePatrolPosition()
    {
        patrolPosition =
            Utils.GetRandomReachablePoint(
                rb.position,
                patrolDistanceMin,
                patrolDistanceMax,
                obstacleMask
            );
    }

    // MOVEMENT
    protected void MoveTo(
        Vector2 target,
        float speed
    )
    {
        Vector2 direction =
            target - rb.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        lastMoveDirection = direction;

        Vector2 newPosition =
            rb.position +
            direction *
            speed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        UpdateSpriteDirection(direction);
    }


    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }


    private void UpdateSpriteDirection(
        Vector2 direction
    )
    {
        if (spriteRenderer == null)
            return;

        if (direction.x > 0.05f)
            spriteRenderer.flipX = false;

        else if (direction.x < -0.05f)
            spriteRenderer.flipX = true;
    }
    // PLAYER
    private void RememberPlayerPosition()
    {
        if (player != null)
        {
            lastKnownPlayerPosition =
                player.position;
        }
    }


    protected bool HasLineOfSight()
    {
        if (player == null)
            return false;

        RaycastHit2D hit =
            Physics2D.Linecast(
                transform.position,
                player.position,
                obstacleMask
            );

        return hit.collider == null;
    }


    protected float GetPlayerDistance()
    {
        if (player == null)
            return Mathf.Infinity;

        return Vector2.Distance(
            transform.position,
            player.position
        );
    }


    private bool CanAttackPlayer()
    {
        if (player == null)
            return false;

        return
            GetPlayerDistance() <= attackDistance &&
            HasLineOfSight();
    }
    // ATTACK

    private void AttackPlayer()
    {
        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(
            attackDamage
        );

        StartCoroutine(
            AttackFlash()
        );
    }


    private IEnumerator AttackFlash()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(0.1f);

        if (currentState != State.Dead)
            spriteRenderer.color = originalColor;
    }
    // DAMAGE
    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead)
            return;

        currentHealth -= damage;

        StartCoroutine(
            DamageFlash()
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        if (currentState != State.Dead)
            spriteRenderer.color = originalColor;
    }


    private void Die()
    {
        ChangeState(State.Dead);

        StopAllCoroutines();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;
    }
    // METHODS FOR CHILD ENEMIES
    protected abstract bool DetectPlayer();

    protected abstract bool CanStillTrackPlayer();


    protected virtual bool ShouldTeleport()
    {
        return false;
    }


    protected virtual void PerformTeleport()
    {
    }


    protected virtual void TickSpecial(float deltaTime)
    {
    }
    // VISUAL DEBUG
    private void OnGUI()
    {
        if (!showDebug)
            return;

        if (Camera.main == null)
            return;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(
                transform.position +
                Vector3.up * 0.8f
            );

        if (screenPosition.z < 0)
            return;

        float x = screenPosition.x - 60f;

        float y =
            Screen.height -
            screenPosition.y;

        GUI.Box(
            new Rect(
                x,
                y,
                120,
                42
            ),
            $"{gameObject.name}\n{currentState}"
        );
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            lastKnownPlayerPosition,
            0.2f
        );

        DrawExtraGizmos();
    }


    protected virtual void DrawExtraGizmos()
    {
    }
}