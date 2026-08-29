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
        foreach (ConfiscationPair pair in _pairs)
            SpawnPair(pair);
    }

    void SpawnPair(ConfiscationPair pair)
    {
        if (pair.spriteA == null || pair.spriteB == null)
            return;

        bool isSideAAnomaly = Random.value < 0.5f;

        SpawnNpc(pair.spriteA, isSideAAnomaly);
        SpawnNpc(pair.spriteB, !isSideAAnomaly);
    }

    void SpawnNpc(GameObject prefab, bool isAnomaly)
    {
        if (NPCBounds.Instance == null || !NavMeshSpawner.TryGetRandomPointInPolygon(NPCBounds.Instance.Center, NPCBounds.Instance.Polygon, out Vector3 point))
            return;

        GameObject npc = Instantiate(prefab, point, Quaternion.identity);
        _spawnedNpcs.Add(npc);

        if (npc.TryGetComponent(out NPCIdentity identity))
            identity.SetIsAnomaly(isAnomaly);
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
}
