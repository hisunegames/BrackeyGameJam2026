using System.Collections.Generic;
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

    readonly List<GameObject> _spawnedNpcs = new();

    void OnEnable()
    {
        ShiftManager.OnNewDayStarted += HandleNewDay;
    }

    void OnDisable()
    {
        ShiftManager.OnNewDayStarted -= HandleNewDay;
    }

    void Start()
    {
        // In case ShiftManager isn't in the scene at all, still spawn once so testing works standalone.
        if (ShiftManager.Instance == null)
            SpawnAll();
    }

    void HandleNewDay(int day)
    {
        DespawnAll();
        SpawnAll();
    }

    void SpawnAll()
    {
        int spawned = 0;

        // Guarantee at least one of each configured normal variant.
        foreach (NpcVariant variant in _npcVariants)
        {
            if (variant.prefab == null || spawned >= _npcCount)
                continue;

            SpawnNpc(variant.prefab, false);
            spawned++;
        }

        // Guarantee at least one anomaly, if any variant supports one.
        if (spawned < _npcCount && TryGetRandomVariantWithAnomaly(out NpcVariant anomalyVariant))
        {
            SpawnNpc(anomalyVariant.anomalyPrefab, true);
            spawned++;
        }

        for (; spawned < _npcCount; spawned++)
            SpawnRandomNpc();
    }

    void SpawnRandomNpc()
    {
        if (!TryGetWeightedVariant(out NpcVariant variant))
            return;

        bool spawnAsAnomaly = variant.anomalyPrefab != null && Random.value < _anomalyChance;
        GameObject prefab = spawnAsAnomaly ? variant.anomalyPrefab : variant.prefab;

        SpawnNpc(prefab, spawnAsAnomaly);
    }

    void SpawnNpc(GameObject prefab, bool isAnomaly)
    {
        if (prefab == null)
            return;

        if (NPCBounds.Instance == null || !NavMeshSpawner.TryGetRandomPointInPolygon(NPCBounds.Instance.Center, NPCBounds.Instance.Polygon, out Vector3 point))
            return;

        GameObject npc = Instantiate(prefab, point, Quaternion.identity);
        _spawnedNpcs.Add(npc);

        if (npc.TryGetComponent(out NPCIdentity identity))
            identity.SetIsAnomaly(isAnomaly);

        if (NPCQuestPoolManager.Instance == null)
            return;

        if (isAnomaly)
            NPCQuestPoolManager.Instance.RegisterAnomalyNpc(npc);
        else
            NPCQuestPoolManager.Instance.RegisterNormalNpc(npc);
    }

    void DespawnAll()
    {
        foreach (GameObject npc in _spawnedNpcs)
        {
            if (npc != null)
                Destroy(npc);
        }

        _spawnedNpcs.Clear();
    }

    bool TryGetRandomVariantWithAnomaly(out NpcVariant variant)
    {
        List<NpcVariant> eligible = new();
        foreach (NpcVariant entry in _npcVariants)
        {
            if (entry.anomalyPrefab != null)
                eligible.Add(entry);
        }

        if (eligible.Count == 0)
        {
            variant = default;
            return false;
        }

        variant = eligible[Random.Range(0, eligible.Count)];
        return true;
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
