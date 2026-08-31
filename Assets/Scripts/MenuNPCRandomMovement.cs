using UnityEngine;

// Simple transform-based wander, bounded by a Collider's XZ footprint instead of a baked NavMesh -
// avoids NavMeshAgent placement/AgentType headaches for a purely decorative menu NPC.
public class MenuNPCRandomMovement : MonoBehaviour
{
    [Header("Walk Area")]
    [SerializeField] Collider _walkArea;

    [Header("Movement")]
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _turnSpeed = 240f;
    [SerializeField] float _arrivalThreshold = 0.2f;

    [Header("Breaks")]
    [SerializeField] float _minBreakDuration = 1f;
    [SerializeField] float _maxBreakDuration = 4f;

    bool _isBreaking;
    float _breakTimer;
    Vector3 _destination;

    void OnEnable()
    {
        StartBreak();
    }

    void Update()
    {
        if (_isBreaking)
        {
            _breakTimer -= Time.deltaTime;
            if (_breakTimer <= 0f)
                PickNewDestination();
            return;
        }

        Vector3 current = transform.position;
        Vector3 toDestination = _destination - current;
        toDestination.y = 0f;

        if (toDestination.magnitude <= _arrivalThreshold)
        {
            StartBreak();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(toDestination.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(current, current + toDestination, _moveSpeed * Time.deltaTime);
    }

    void StartBreak()
    {
        _isBreaking = true;
        _breakTimer = Random.Range(_minBreakDuration, _maxBreakDuration);
    }

    void PickNewDestination()
    {
        if (_walkArea == null)
        {
            _breakTimer = Random.Range(_minBreakDuration, _maxBreakDuration);
            return;
        }

        Bounds bounds = _walkArea.bounds;
        _destination = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y,
            Random.Range(bounds.min.z, bounds.max.z));

        _isBreaking = false;
    }
}
