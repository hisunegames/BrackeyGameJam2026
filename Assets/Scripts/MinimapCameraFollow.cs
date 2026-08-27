using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _height = 20f;
    [SerializeField] bool _matchPlayerRotation = false;
    [SerializeField] float _rotationOffset = 0f;

    [Header("World Bounds (Polygon)")]
    [SerializeField] bool _clampToBounds = false;
    [SerializeField] Vector2[] _boundsPolygon;

    [Header("Performance")]
    [SerializeField] float _refreshInterval = 1f;

    Camera _camera;
    float _refreshTimer;

    void Awake()
    {
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _target = player.transform;
        }

        _camera = GetComponent<Camera>();
        if (_camera != null)
            _camera.enabled = false;
    }

    void LateUpdate()
    {
        if (_target == null)
            return;

        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer > 0f)
            return;

        _refreshTimer = _refreshInterval;

        Vector3 followPosition = _target.position;
        if (_clampToBounds && _boundsPolygon != null && _boundsPolygon.Length >= 3)
        {
            Vector2 clamped = ClampToPolygon(new Vector2(followPosition.x, followPosition.z), _boundsPolygon);
            followPosition.x = clamped.x;
            followPosition.z = clamped.y;
        }

        transform.position = followPosition + Vector3.up * _height;
        transform.rotation = _matchPlayerRotation
            ? Quaternion.Euler(90f, _target.eulerAngles.y + _rotationOffset, 0f)
            : Quaternion.Euler(90f, 0f, 0f);

        if (_camera != null)
            _camera.Render();
    }

    static Vector2 ClampToPolygon(Vector2 point, Vector2[] polygon)
    {
        if (IsPointInPolygon(point, polygon))
            return point;

        Vector2 closest = polygon[0];
        float closestDistSq = float.MaxValue;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];
            Vector2 candidate = ClosestPointOnSegment(point, a, b);
            float distSq = (point - candidate).sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closest = candidate;
            }
        }
        return closest;
    }

    static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float sqrLength = Vector2.Dot(ab, ab);
        if (sqrLength < 0.0001f)
            return a;

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / sqrLength);
        return a + ab * t;
    }

    static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];
            if ((pi.y > point.y) != (pj.y > point.y) &&
                point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x)
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }

    void OnDrawGizmosSelected()
    {
        if (!_clampToBounds || _boundsPolygon == null || _boundsPolygon.Length < 2)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _boundsPolygon.Length; i++)
        {
            Vector2 a = _boundsPolygon[i];
            Vector2 b = _boundsPolygon[(i + 1) % _boundsPolygon.Length];
            Gizmos.DrawLine(new Vector3(a.x, 0f, a.y), new Vector3(b.x, 0f, b.y));
        }
    }
}
