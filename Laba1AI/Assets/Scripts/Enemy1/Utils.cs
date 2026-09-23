using UnityEngine;

public static class Utils
{
    public static Vector2 GetRandomDirection2D()
    {
        return Random.insideUnitCircle.normalized;
    }

    public static Vector2 GetRandomPosition2D(
        Vector2 startingPosition,
        float minDistance,
        float maxDistance
    )
    {
        Vector2 randomDirection =
            GetRandomDirection2D();

        float distance = Random.Range(
            minDistance,
            maxDistance
        );

        return startingPosition +
               randomDirection * distance;
    }
}