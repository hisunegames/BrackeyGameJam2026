using UnityEngine;

public class EnemyLineOfSight : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform _target;
    [SerializeField] Transform _eyePoint;

    [Header("Detection Range")]
    [SerializeField] float _viewDistance = 15f;
    [SerializeField] float _crouchViewDistance = 6f;
    [SerializeField, Range(0f, 360f)] float _viewAngle = 90f;
    [SerializeField] LayerMask _obstructionLayers = ~0;

    [Header("Presence Detection")]
    [SerializeField] float _presenceRadius = 3f;
    [SerializeField] float _crouchPresenceRadius = 1.5f;

    public bool CanSeePlayer { get; private set; }
    public bool IsPlayerInPresenceRange { get; private set; }
    public bool HasDetectedPlayer => CanSeePlayer || IsPlayerInPresenceRange;

    void Awake()
    {
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _target = player.transform;
        }
    }

    void Update()
    {
        UpdateDetection();
    }

    void UpdateDetection()
    {
        if (_target == null)
        {
            CanSeePlayer = false;
            IsPlayerInPresenceRange = false;
            return;
        }

        Vector3 eyePos = EyePosition;
        Vector3 toTarget = _target.position - eyePos;
        float distance = toTarget.magnitude;

        bool isCrouching = _target.TryGetComponent(out PlayerController player) && player.IsCrouching;

        float presenceRadius = isCrouching ? _crouchPresenceRadius : _presenceRadius;
        IsPlayerInPresenceRange = distance <= presenceRadius;

        CanSeePlayer = CheckLineOfSight(eyePos, toTarget, distance, isCrouching);
    }

    bool CheckLineOfSight(Vector3 eyePos, Vector3 toTarget, float distance, bool isCrouching)
    {
        float maxDistance = isCrouching ? _crouchViewDistance : _viewDistance;
        if (distance > maxDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle > _viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(eyePos, toTarget.normalized, out RaycastHit hit, distance, _obstructionLayers, QueryTriggerInteraction.Ignore)
            && hit.transform.root != _target.root)
        {
            return false;
        }

        return true;
    }

    Vector3 EyePosition => _eyePoint != null ? _eyePoint.position : transform.position;

    void OnDrawGizmosSelected()
    {
        Vector3 eyePos = EyePosition;

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        DrawViewCone(eyePos, _crouchViewDistance);

        Gizmos.color = HasDetectedPlayer ? Color.red : Color.yellow;
        DrawViewCone(eyePos, _viewDistance);

        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawWireSphere(eyePos, _crouchPresenceRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(eyePos, _presenceRadius);

        if (HasDetectedPlayer && _target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePos, _target.position);
        }
    }

    void DrawViewCone(Vector3 origin, float distance)
    {
        Vector3 forward = transform.forward;
        float halfAngle = _viewAngle * 0.5f;

        Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward;
        Vector3 rightDirection = Quaternion.AngleAxis(halfAngle, Vector3.up) * forward;

        Gizmos.DrawRay(origin, leftDirection * distance);
        Gizmos.DrawRay(origin, rightDirection * distance);

        const int segments = 24;
        Vector3 prevPoint = origin + leftDirection * distance;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 direction = Quaternion.AngleAxis(Mathf.Lerp(-halfAngle, halfAngle, t), Vector3.up) * forward;
            Vector3 point = origin + direction * distance;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
