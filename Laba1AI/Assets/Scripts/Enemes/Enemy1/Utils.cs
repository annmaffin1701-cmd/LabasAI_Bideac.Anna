using UnityEngine;

public static class Utils
{
    public static Vector2 GetRandomDirection2D()
    {
        return Random.insideUnitCircle.normalized;
    }

    public static Vector2 GetRandomReachablePoint(
        Vector2 origin,
        float minDistance,
        float maxDistance,
        LayerMask obstacleMask,
        int attempts = 15)
    {
        for (int i = 0; i < attempts; i++)
        {
            Vector2 direction = GetRandomDirection2D();

            float distance = Random.Range(
                minDistance,
                maxDistance
            );

            Vector2 candidate =
                origin + direction * distance;

            // “очка не должна находитьс€ внутри стены
            Collider2D obstacle = Physics2D.OverlapCircle(
                candidate,
                0.2f,
                obstacleMask
            );

            if (obstacle != null)
                continue;

            // ћежду врагом и точкой не должно быть стены
            RaycastHit2D hit = Physics2D.Linecast(
                origin,
                candidate,
                obstacleMask
            );

            if (hit.collider != null)
                continue;

            return candidate;
        }

        return origin;
    }
}