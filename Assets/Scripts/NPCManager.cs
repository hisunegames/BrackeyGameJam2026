using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    struct NpcVariant
    {
        // Multiple entries here are purely cosmetic (e.g. outfit swaps) - one gets picked at
        // random per spawn. They all still count as the same character for CharacterId purposes.
        public GameObject[] spritePrefabs;

        // Rare cosmetic swap, independent of IsAnomaly - structurally like the old anomaly
        // prefab/chance, but purely a look, not a tell.
        public GameObject specialVariantPrefab;
        [Range(0f, 1f)] public float specialVariantChance;

        public float weight;
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
            if (!HasSprites(variant) || spawned >= _npcCount)
                continue;

            SpawnNpc(variant, false);
            spawned++;
        }

        // Guarantee at least one anomaly - same variant pool as everyone else, since appearance
        // is purely cosmetic and shouldn't be a tell.
        if (spawned < _npcCount && TryGetWeightedVariant(out NpcVariant anomalyVariant))
        {
            SpawnNpc(anomalyVariant, true);
            spawned++;
        }

        for (; spawned < _npcCount; spawned++)
            SpawnRandomNpc();
    }

    void SpawnRandomNpc()
    {
        if (!TryGetWeightedVariant(out NpcVariant variant))
            return;

        bool spawnAsAnomaly = Random.value < _anomalyChance;
        SpawnNpc(variant, spawnAsAnomaly);
    }

    void SpawnNpc(NpcVariant variant, bool isAnomaly)
    {
        GameObject prefab = GetRandomSprite(variant);
        if (prefab == null || !SpawnerUtility.TrySpawnInBounds(prefab, out GameObject npc))
            return;

        _spawnedNpcs.Add(npc);

        if (npc.TryGetComponent(out NPCIdentity identity))
        {
            // Stable per-variant id (not per-sprite, and not the special variant either) so any
            // cosmetic sprite swap of the same character still registers as a duplicate for the
            // quest-pool's dedup logic.
            identity.SetCharacterId(GetCharacterId(variant));
            identity.SetIsAnomaly(isAnomaly);
        }

        if (NPCQuestPoolManager.Instance == null)
            return;

        if (isAnomaly)
            NPCQuestPoolManager.Instance.RegisterAnomalyNpc(npc);
        else
            NPCQuestPoolManager.Instance.RegisterNormalNpc(npc);
    }

    void DespawnAll() => SpawnerUtility.DestroyAndClear(_spawnedNpcs);

    bool HasSprites(NpcVariant variant) => variant.spritePrefabs != null && variant.spritePrefabs.Length > 0;

    GameObject GetRandomSprite(NpcVariant variant)
    {
        // Rare cosmetic swap, rolled independently of IsAnomaly - a look, not a tell.
        if (variant.specialVariantPrefab != null && Random.value < variant.specialVariantChance)
            return variant.specialVariantPrefab;

        if (!HasSprites(variant))
            return null;

        return variant.spritePrefabs[Random.Range(0, variant.spritePrefabs.Length)];
    }

    string GetCharacterId(NpcVariant variant)
    {
        if (HasSprites(variant))
            return variant.spritePrefabs[0].name;

        return variant.specialVariantPrefab != null ? variant.specialVariantPrefab.name : string.Empty;
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
