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

        if (NPCBounds.Instance == null || !NavMeshSpawner.TryGetRandomPointInPolygon(NPCBounds.Instance.Center, NPCBounds.Instance.Polygon, out Vector3 point))
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
}
