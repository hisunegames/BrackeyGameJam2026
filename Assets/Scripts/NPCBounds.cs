using UnityEngine;

public class NPCBounds : MonoBehaviour
{
    public static NPCBounds Instance { get; private set; }

    [SerializeField] Vector2[] _polygon;

    public Vector3 Center => transform.position;
    public Vector2[] Polygon => _polygon;

    void Awake()
    {
        Instance = this;
    }

    void OnDrawGizmosSelected()
    {
        if (_polygon == null || _polygon.Length < 2)
            return;

        Gizmos.color = Color.magenta;
        Vector3 center = transform.position;
        for (int i = 0; i < _polygon.Length; i++)
        {
            Vector2 a = _polygon[i];
            Vector2 b = _polygon[(i + 1) % _polygon.Length];
            Gizmos.DrawLine(center + new Vector3(a.x, 0f, a.y), center + new Vector3(b.x, 0f, b.y));
        }
    }
}
