using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Pool")]
    [SerializeField] GameObject[] _npcVariants;

    [Header("Spawn Settings")]
    [SerializeField] int _npcCount = 10;
    [SerializeField] Transform _spawnCenter;
    [SerializeField] float _spawnRadius = 10f;

    Vector3 SpawnCenter => _spawnCenter != null ? _spawnCenter.position : transform.position;

    void Start()
    {
        for (int i = 0; i < _npcCount; i++)
            SpawnNpc();
    }

    void SpawnNpc()
    {
        if (_npcVariants.Length == 0)
            return;

        if (!NavMeshSpawner.TryGetRandomPoint(SpawnCenter, _spawnRadius, out Vector3 point))
            return;

        GameObject prefab = _npcVariants[Random.Range(0, _npcVariants.Length)];
        Instantiate(prefab, point, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(SpawnCenter, _spawnRadius);
    }
}
