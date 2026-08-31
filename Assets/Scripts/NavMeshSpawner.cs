using UnityEngine;
using UnityEngine.AI;

public static class NavMeshSpawner
{
    public static bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public static bool TryGetRandomPointInEllipse(Vector3 center, float radiusX, float radiusZ, out Vector3 result)
    {
        float angle = Random.value * Mathf.PI * 2f;
        float distance = Mathf.Sqrt(Random.value);
        Vector3 offset = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ) * distance;
        Vector3 randomPoint = center + offset;

        float sampleRadius = Mathf.Max(radiusX, radiusZ);
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public static bool TryGetRandomPointInPolygon(Vector3 center, Vector2[] polygonOffsets, out Vector3 result, int maxAttempts = 30)
        => TryGetRandomPointInPolygon(center, polygonOffsets, Vector3.zero, null, out result, maxAttempts);

    // Overload that also requires the sampled point to fall within a SECOND polygon (e.g. the
    // overall level bounds), staying close in height to the PRIMARY center (the caller's actual
    // anchor, e.g. the player) - not the secondary polygon's own transform, which is just a
    // bounding marker and isn't necessarily placed at real floor height. For spawns that need to
    // stay both near some local area AND inside the map on the SAME floor, like a penalty monster
    // spawning near the player but never past the outer walls or onto a different floor
    // (NavMesh.SamplePosition's search radius can otherwise snap onto a NavMesh a level above/below
    // near stairs/gaps).
    public static bool TryGetRandomPointInPolygon(Vector3 center, Vector2[] polygonOffsets, Vector3 secondaryPolygonCenter, Vector2[] secondaryPolygonOffsets, out Vector3 result, int maxAttempts = 30, float maxAnchorHeightDifference = 3f)
    {
        if (polygonOffsets == null || polygonOffsets.Length < 3)
        {
            result = Vector3.zero;
            return false;
        }

        Vector2 min = polygonOffsets[0];
        Vector2 max = polygonOffsets[0];
        foreach (Vector2 point in polygonOffsets)
        {
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        bool hasAnchor = secondaryPolygonOffsets != null && secondaryPolygonOffsets.Length >= 3;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomOffset = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (!IsPointInPolygon(randomOffset, polygonOffsets))
                continue;

            Vector3 worldPoint = center + new Vector3(randomOffset.x, 0f, randomOffset.y);

            if (hasAnchor)
            {
                Vector2 secondaryOffset = new Vector2(worldPoint.x - secondaryPolygonCenter.x, worldPoint.z - secondaryPolygonCenter.z);
                if (!IsPointInPolygon(secondaryOffset, secondaryPolygonOffsets))
                    continue;
            }

            if (NavMesh.SamplePosition(worldPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                if (hasAnchor && Mathf.Abs(hit.position.y - center.y) > maxAnchorHeightDifference)
                    continue;

                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    public static bool TryGetRandomPointInRadiusWithinPolygon(Vector3 selfPosition, float radius, Vector3 polygonCenter, Vector2[] polygonOffsets, out Vector3 result, int maxAttempts = 30)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float distance = Mathf.Sqrt(Random.value) * radius;
            Vector3 candidate = selfPosition + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            if (polygonOffsets != null && polygonOffsets.Length >= 3)
            {
                Vector2 offset = new Vector2(candidate.x - polygonCenter.x, candidate.z - polygonCenter.z);
                if (!IsPointInPolygon(offset, polygonOffsets))
                    continue;
            }

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];
            if ((pi.y > point.y) != (pj.y > point.y) &&
                point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x)
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }
}
