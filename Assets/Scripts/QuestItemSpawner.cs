using UnityEngine;

public class QuestItemSpawner : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] GameObject[] _itemVariants;

    [Header("Spawn Settings")]
    [SerializeField] int _itemCount = 5;
    [SerializeField] Transform _spawnCenter;
    [SerializeField] float _spawnRadiusX = 10f;
    [SerializeField] float _spawnRadiusZ = 10f;

    Vector3 SpawnCenter => _spawnCenter != null ? _spawnCenter.position : transform.position;

    void Start()
    {
        for (int i = 0; i < _itemCount; i++)
            SpawnItem();
    }

    void SpawnItem()
    {
        if (_itemVariants.Length == 0)
            return;

        if (!NavMeshSpawner.TryGetRandomPointInEllipse(SpawnCenter, _spawnRadiusX, _spawnRadiusZ, out Vector3 point))
            return;

        GameObject prefab = _itemVariants[Random.Range(0, _itemVariants.Length)];
        Instantiate(prefab, point, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

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
