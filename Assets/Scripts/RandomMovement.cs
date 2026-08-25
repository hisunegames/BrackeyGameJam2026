using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomMovement : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] float _wanderRadius = 10f;
    [SerializeField] float _minPauseTime = 3f;
    [SerializeField] float _maxPauseTime = 8f;

    NavMeshAgent _agent;
    Vector3 _originPosition;
    bool _isWaiting;
    float _waitTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _originPosition = transform.position;
    }

    void Start()
    {
        MoveToNewDestination();
    }

    void Update()
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
                MoveToNewDestination();
        }
        else if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(_minPauseTime, _maxPauseTime);
        }
    }

    void MoveToNewDestination()
    {
        _isWaiting = false;

        if (NavMeshSpawner.TryGetRandomPoint(_originPosition, _wanderRadius, out Vector3 point))
            _agent.SetDestination(point);
    }
}
