using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    struct NpcVariant
    {
        public GameObject prefab;
        public float weight;
        public GameObject anomalyPrefab;
    }

    [Header("NPC Pool")]
    [SerializeField] NpcVariant[] _npcVariants;

    [Header("Anomaly Settings")]
    [SerializeField, Range(0f, 1f)] float _anomalyChance = 0.05f;

    [Header("Spawn Settings")]
    [SerializeField] int _npcCount = 10;
    [SerializeField] Transform _spawnCenter;
    [SerializeField] float _spawnRadiusX = 10f;
    [SerializeField] float _spawnRadiusZ = 6f;

    Vector3 SpawnCenter => _spawnCenter != null ? _spawnCenter.position : transform.position;

    void Start()
    {
        for (int i = 0; i < _npcCount; i++)
            SpawnNpc();
    }

    void SpawnNpc()
    {
        if (!TryGetWeightedVariant(out NpcVariant variant))
            return;

        GameObject prefab = variant.anomalyPrefab != null && Random.value < _anomalyChance
            ? variant.anomalyPrefab
            : variant.prefab;

        if (!NavMeshSpawner.TryGetRandomPointInEllipse(SpawnCenter, _spawnRadiusX, _spawnRadiusZ, out Vector3 point))
            return;

        Instantiate(prefab, point, Quaternion.identity);
    }

    bool TryGetWeightedVariant(out NpcVariant variant)
    {
        float totalWeight = 0f;
        foreach (NpcVariant entry in _npcVariants)
            totalWeight += entry.weight;

        if (totalWeight <= 0f)
        {
            variant = default;
            return false;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (NpcVariant entry in _npcVariants)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
            {
                variant = entry;
                return true;
            }
        }

        variant = _npcVariants[_npcVariants.Length - 1];
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        const int segments = 48;
        Vector3 center = SpawnCenter;
        Vector3 prevPoint = center + new Vector3(_spawnRadiusX, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * _spawnRadiusX, 0f, Mathf.Sin(angle) * _spawnRadiusZ);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
