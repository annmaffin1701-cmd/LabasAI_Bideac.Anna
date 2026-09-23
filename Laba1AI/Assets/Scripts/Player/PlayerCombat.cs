using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Enemy")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerInput playerInput;

    private float attackTimer;


    private void Awake()
    {
        playerInput =
            new PlayerInput();
    }


    private void OnEnable()
    {
        playerInput.Enable();

        playerInput.Player.Attack.performed +=
            OnAttack;
    }


    private void OnDisable()
    {
        playerInput.Player.Attack.performed -=
            OnAttack;

        playerInput.Disable();
    }


    private void Update()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }


    private void OnAttack(
        InputAction.CallbackContext context
    )
    {
        if (attackTimer > 0f)
            return;

        Attack();

        attackTimer =
            attackCooldown;
    }


    private void Attack()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                transform.position,
                attackRadius,
                enemyLayer
            );

        foreach (
            Collider2D enemyCollider
            in enemies)
        {
            EnemyFSMBase enemy =
                enemyCollider
                    .GetComponentInParent
                    <EnemyFSMBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(
                    attackDamage
                );
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRadius
        );
    }
}