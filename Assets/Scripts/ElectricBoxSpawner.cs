using System.Collections.Generic;
using UnityEngine;

public class ElectricBoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject _electricBoxPrefab;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] Vector3 _rotationOffset;

    public static ElectricBoxSpawner Instance { get; private set; }

    GameObject _spawnedElectricBox;
    readonly List<GameObject> _debugSpawnedBoxes = new();
    bool _debugBoxesActive;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnSequenceChanged += HandleSequenceChanged;
    }

    void OnDisable()
    {
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        if (sequence == GameplaySequence.Penalty && _spawnedElectricBox == null)
            SpawnElectricBox();
        else if (sequence == GameplaySequence.Normal)
            DespawnElectricBox();
    }

    void SpawnElectricBox()
    {
        if (_electricBoxPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            return;

        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(_rotationOffset);
        _spawnedElectricBox = Instantiate(_electricBoxPrefab, spawnPoint.position, rotation);
    }

    void DespawnElectricBox()
    {
        if (_spawnedElectricBox != null)
            Destroy(_spawnedElectricBox);

        _spawnedElectricBox = null;
    }

    public void DebugToggleSpawnAll()
    {
        if (_debugBoxesActive)
            DebugDespawnAll();
        else
            DebugSpawnAll();
    }

    void DebugSpawnAll()
    {
        DebugDespawnAll();

        if (_electricBoxPrefab == null || _spawnPoints == null)
            return;

        foreach (Transform point in _spawnPoints)
        {
            if (point == null)
                continue;

            Quaternion rotation = point.rotation * Quaternion.Euler(_rotationOffset);
            _debugSpawnedBoxes.Add(Instantiate(_electricBoxPrefab, point.position, rotation));
        }

        _debugBoxesActive = true;
    }

    void DebugDespawnAll()
    {
        foreach (GameObject box in _debugSpawnedBoxes)
        {
            if (box != null)
                Destroy(box);
        }

        _debugSpawnedBoxes.Clear();
        _debugBoxesActive = false;
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
        if (_electricBoxPrefab == null)
            return null;

        Renderer[] renderers = _electricBoxPrefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return null;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}
