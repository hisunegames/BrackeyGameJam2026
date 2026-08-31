using System.Collections.Generic;
using UnityEngine;

public class ConfiscationNpcSpawner : MonoBehaviour
{
    [System.Serializable]
    struct ConfiscationPair
    {
        public GameObject spriteA;
        public GameObject spriteB;
    }

    [SerializeField] ConfiscationPair[] _pairs;

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
        // Same prefab can end up referenced by more than one _pairs entry - only spawn its
        // pair-set the first time it's seen, so a given variant never appears more than once.
        HashSet<GameObject> spawnedVariants = new();

        foreach (ConfiscationPair pair in _pairs)
            SpawnPair(pair, spawnedVariants);
    }

    void SpawnPair(ConfiscationPair pair, HashSet<GameObject> spawnedVariants)
    {
        if (pair.spriteA == null || pair.spriteB == null)
            return;

        if (spawnedVariants.Contains(pair.spriteA) || spawnedVariants.Contains(pair.spriteB))
            return;

        spawnedVariants.Add(pair.spriteA);
        spawnedVariants.Add(pair.spriteB);

        bool isSideAAnomaly = Random.value < 0.5f;

        SpawnNpc(pair.spriteA, isSideAAnomaly);
        SpawnNpc(pair.spriteB, !isSideAAnomaly);
    }

    void SpawnNpc(GameObject prefab, bool isAnomaly)
    {
        if (!SpawnerUtility.TrySpawnInBounds(prefab, out GameObject npc))
            return;

        _spawnedNpcs.Add(npc);

        if (npc.TryGetComponent(out NPCIdentity identity))
        {
            // Stable per-prefab id, same convention as NPCManager - lets the pool tell duplicate
            // spawns of the same confiscation variant apart from genuinely different NPCs.
            identity.SetCharacterId(prefab.name);
            identity.SetIsAnomaly(isAnomaly);
        }

        // Feeds confiscation NPCs into the same quest-giver/notepad pipeline NPCManager uses -
        // matched by their own NPCQuestMarker just like fetch-item NPCs. Not registering the
        // anomaly side as THE anomaly decoy here - that single slot belongs to NPCManager's own
        // pick; ResolveNoteVariant already keeps this NPC's note null regardless if it's anomaly.
        if (NPCQuestPoolManager.Instance != null)
            NPCQuestPoolManager.Instance.RegisterNormalNpc(npc);
    }

    void DespawnAll() => SpawnerUtility.DestroyAndClear(_spawnedNpcs);
}
