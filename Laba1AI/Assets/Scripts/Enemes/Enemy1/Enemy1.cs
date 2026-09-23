using UnityEngine;

public class Enemy1 : EnemyFSMBase
{
    [Header("Normal Enemy Vision")]
    [SerializeField] private float detectionDistance = 6f;

    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 180f;


    protected override bool DetectPlayer()
    {
        return IsPlayerInsideVision();
    }


    protected override bool CanStillTrackPlayer()
    {
        return IsPlayerInsideVision();
    }


    private bool IsPlayerInsideVision()
    {
        if (player == null)
            return false;

        Vector2 directionToPlayer =
            (Vector2)player.position -
            rb.position;

        float distance =
            directionToPlayer.magnitude;

        // Проверка дальности
        if (distance > detectionDistance)
            return false;

        directionToPlayer.Normalize();

        Vector2 facingDirection =
            lastMoveDirection.normalized;

        if (facingDirection == Vector2.zero)
            facingDirection = Vector2.right;

        // Проверка угла
        float angle =
            Vector2.Angle(
                facingDirection,
                directionToPlayer
            );

        if (angle > viewAngle * 0.5f)
            return false;

        // Проверка стены
        if (!HasLineOfSight())
            return false;

        return true;
    }


    protected override void DrawExtraGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionDistance
        );

        Vector2 direction =
            lastMoveDirection;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        Vector3 left =
            Quaternion.Euler(
                0,
                0,
                viewAngle / 2f
            ) * direction;

        Vector3 right =
            Quaternion.Euler(
                0,
                0,
                -viewAngle / 2f
            ) * direction;

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            left * detectionDistance
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            right * detectionDistance
        );
    }
}