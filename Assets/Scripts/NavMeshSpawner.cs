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
}
