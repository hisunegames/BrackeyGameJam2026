using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class QuestItemSpawner : MonoBehaviour
{
    [System.Serializable]
    struct QuestItemEntry
    {
        public GameObject prefab;
        public string questName;
    }

    [Header("Item Pool")]
    [SerializeField] QuestItemEntry[] _itemVariants;

    [Header("Spawn Settings")]
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] Vector3 _rotationOffset;

    readonly List<Transform> _availablePoints = new();
    readonly HashSet<string> _spawnedQuestNames = new();
    readonly List<GameObject> _spawnedItems = new();

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
        RefillPoints();
    }

    // Without this, a quest name that recurs on a later day (ShiftManager only avoids duplicates
    // within a single day's picks) would stay stuck in _spawnedQuestNames forever and never spawn
    // its item again, and _availablePoints would eventually run dry since it's only ever drained.
    void HandleNewDay(int day)
    {
        SpawnerUtility.DestroyAndClear(_spawnedItems);
        _spawnedQuestNames.Clear();
        RefillPoints();
    }

    void RefillPoints()
    {
        _availablePoints.Clear();
        if (_spawnPoints != null)
        {
            _availablePoints.AddRange(_spawnPoints);
            SpawnerUtility.Shuffle(_availablePoints);
        }
    }

    void Update()
    {
        if (_itemVariants == null)
            return;

        foreach (QuestItemEntry entry in _itemVariants)
        {
            if (entry.prefab == null || string.IsNullOrEmpty(entry.questName) || _spawnedQuestNames.Contains(entry.questName))
                continue;

            if (!QuestLog.IsQuestInStateMask(entry.questName, QuestState.Active))
                continue;

            _spawnedQuestNames.Add(entry.questName);
            SpawnItem(entry.prefab);
        }
    }

    void SpawnItem(GameObject prefab)
    {
        if (_availablePoints.Count == 0)
            return;

        Transform spawnPoint = _availablePoints[0];
        _availablePoints.RemoveAt(0);

        if (spawnPoint == null)
            return;

        Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(_rotationOffset);
        _spawnedItems.Add(Instantiate(prefab, spawnPoint.position, rotation));
    }

    void OnDrawGizmosSelected()
    {
        if (_spawnPoints == null)
            return;

        foreach (Transform point in _spawnPoints)
        {
            if (point == null)
                continue;

            Quaternion rotation = point.rotation * Quaternion.Euler(_rotationOffset);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point.position, 0.3f);
            DrawDirectionArrow(point.position, rotation * Vector3.forward, 0.75f);
        }
    }

    void DrawDirectionArrow(Vector3 origin, Vector3 direction, float length)
    {
        Vector3 tip = origin + direction * length;
        Gizmos.DrawLine(origin, tip);

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rightHead = lookRotation * Quaternion.Euler(0f, 155f, 0f) * Vector3.forward;
        Vector3 leftHead = lookRotation * Quaternion.Euler(0f, -155f, 0f) * Vector3.forward;

        float headLength = length * 0.35f;
        Gizmos.DrawLine(tip, tip + rightHead * headLength);
        Gizmos.DrawLine(tip, tip + leftHead * headLength);
    }
}
