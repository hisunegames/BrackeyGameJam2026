using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotepadPodiumSpawner : MonoBehaviour
{
    [SerializeField] GameObject _notepadPrefab;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] Vector3 _rotationOffset;
    [SerializeField] int _podiumCount = 2;

    public static NotepadPodiumSpawner Instance { get; private set; }

    readonly List<GameObject> _spawnedNotepads = new();
    readonly List<GameObject> _debugSpawnedNotepads = new();
    bool _debugNotepadsActive;

    void Awake()
    {
        Instance = this;
    }

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

        // NPCManager registers the day's NPCs synchronously (same frame as this event), but
        // NPCQuestPoolManager now defers its own matching by one frame for the same reason we
        // defer here - so this needs to wait two frames total to run after THAT, not race it.
        StartCoroutine(SpawnAllNextFrame());
    }

    IEnumerator SpawnAllNextFrame()
    {
        yield return null;
        yield return null;
        SpawnAll();
    }

    void SpawnAll()
    {
        if (_notepadPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            return;

        List<NoteData> notes = CollectDayNotes();

        int podiumCount = Mathf.Max(1, Mathf.Min(_podiumCount, _spawnPoints.Length));
        if (notes.Count > 0)
            podiumCount = Mathf.Min(podiumCount, notes.Count);

        List<Transform> points = new(_spawnPoints);
        SpawnerUtility.Shuffle(points);

        List<NoteData>[] buckets = SplitNotes(notes, podiumCount);
        for (int i = 0; i < podiumCount; i++)
            SpawnPodium(points[i], buckets[i]);
    }

    void SpawnPodium(Transform spawnPoint, List<NoteData> notes)
    {
        if (spawnPoint == null)
            return;

        Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(_rotationOffset);
        GameObject podium = Instantiate(_notepadPrefab, spawnPoint.position, rotation);
        _spawnedNotepads.Add(podium);

        NotepadHandler handler = podium.GetComponentInChildren<NotepadHandler>();
        if (handler != null)
            handler.SetNotes(notes.ToArray());
    }

    List<NoteData>[] SplitNotes(List<NoteData> notes, int bucketCount)
    {
        List<NoteData>[] buckets = new List<NoteData>[bucketCount];
        for (int i = 0; i < bucketCount; i++)
            buckets[i] = new List<NoteData>();

        for (int i = 0; i < notes.Count; i++)
            buckets[i % bucketCount].Add(notes[i]);

        return buckets;
    }

    List<NoteData> CollectDayNotes()
    {
        List<NoteData> notes = new();

        if (NPCQuestPoolManager.Instance != null)
        {
            foreach (GameObject npc in NPCQuestPoolManager.Instance.ActiveQuestGivers)
                AddNoteFor(npc, notes);

            foreach (GameObject anomalyNpc in NPCQuestPoolManager.Instance.AnomalyNpcs)
                AddNoteFor(anomalyNpc, notes);
        }

        return notes;
    }

    void AddNoteFor(GameObject npc, List<NoteData> notes)
    {
        if (npc != null && npc.TryGetComponent(out NPCIdentity identity) && identity.AssociatedNote != null)
            notes.Add(identity.AssociatedNote);
    }

    // Debug only - re-collects the day's notes and re-injects them into whatever podiums are
    // already spawned, without respawning them. Needed because SetNotes() only runs once at
    // spawn time, so a debug pool-fill (e.g. F12) done afterward wouldn't otherwise reach them.
    public void DebugRefreshNotes()
    {
        int podiumCount = _spawnedNotepads.Count;
        if (podiumCount == 0)
            return;

        List<NoteData> notes = CollectDayNotes();
        List<NoteData>[] buckets = SplitNotes(notes, podiumCount);

        for (int i = 0; i < podiumCount; i++)
        {
            if (_spawnedNotepads[i] == null)
                continue;

            NotepadHandler handler = _spawnedNotepads[i].GetComponentInChildren<NotepadHandler>();
            if (handler != null)
                handler.SetNotes(buckets[i].ToArray());
        }
    }

    void DespawnAll() => SpawnerUtility.DestroyAndClear(_spawnedNotepads);

    public void DebugToggleSpawnAll()
    {
        if (_debugNotepadsActive)
            DebugDespawnAll();
        else
            DebugSpawnAll();
    }

    void DebugSpawnAll()
    {
        DebugDespawnAll();

        if (_notepadPrefab == null || _spawnPoints == null)
            return;

        foreach (Transform point in _spawnPoints)
        {
            if (point == null)
                continue;

            Quaternion rotation = point.rotation * Quaternion.Euler(_rotationOffset);
            _debugSpawnedNotepads.Add(Instantiate(_notepadPrefab, point.position, rotation));
        }

        _debugNotepadsActive = true;
    }

    void DebugDespawnAll()
    {
        SpawnerUtility.DestroyAndClear(_debugSpawnedNotepads);
        _debugNotepadsActive = false;
    }

    void OnDrawGizmosSelected()
    {
        if (_spawnPoints == null)
            return;

        Bounds? prefabBounds = GetPrefabLocalBounds();
        Matrix4x4 defaultMatrix = Gizmos.matrix;

        foreach (Transform point in _spawnPoints)
        {
            if (point == null)
                continue;

            Quaternion rotation = point.rotation * Quaternion.Euler(_rotationOffset);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point.position, 0.3f);
            DrawDirectionArrow(point.position, rotation * Vector3.forward, 0.75f);

            if (prefabBounds.HasValue)
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = Matrix4x4.TRS(point.position, rotation, Vector3.one);
                Gizmos.DrawWireCube(prefabBounds.Value.center, prefabBounds.Value.size);
                Gizmos.matrix = defaultMatrix;
            }
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

    Bounds? GetPrefabLocalBounds()
    {
        if (_notepadPrefab == null)
            return null;

        Renderer[] renderers = _notepadPrefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return null;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}
