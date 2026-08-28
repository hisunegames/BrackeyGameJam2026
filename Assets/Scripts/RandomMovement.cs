using UnityEngine;
using UnityEngine.AI;

public enum WanderMode
{
    PlayerBiased,
    LocalRadius
}

[RequireComponent(typeof(NavMeshAgent))]
public class RandomMovement : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] WanderMode _wanderMode = WanderMode.PlayerBiased;
    [SerializeField] float _minPauseTime = 3f;
    [SerializeField] float _maxPauseTime = 8f;

    [Header("Player-Biased Wander")]
    [SerializeField] Transform _player;
    [SerializeField] int _candidateCount = 5;
    [SerializeField] float _playerBiasStrength = 2f;

    [Header("Local Radius Wander")]
    [SerializeField] float _localRadius = 8f;

    NavMeshAgent _agent;
    bool _isWaiting;
    float _waitTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_player == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _player = player.transform;
        }
    }

    void Start()
    {
        MoveToNewDestination();
    }

    void Update()
    {
        if (_agent.isStopped)
            return;

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

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        _agent.isStopped = true;
    }

    void OnConversationEnd(Transform actor)
    {
        _agent.isStopped = false;
    }

    void MoveToNewDestination()
    {
        _isWaiting = false;

        Vector3 point;
        bool found = _wanderMode == WanderMode.PlayerBiased
            ? TryGetWeightedWanderPoint(out point)
            : TryGetLocalRadiusPoint(out point);

        if (found)
            _agent.SetDestination(point);
    }

    bool TryGetLocalRadiusPoint(out Vector3 result)
    {
        Vector3 polygonCenter = NPCBounds.Instance != null ? NPCBounds.Instance.Center : transform.position;
        Vector2[] polygon = NPCBounds.Instance != null ? NPCBounds.Instance.Polygon : null;

        return NavMeshSpawner.TryGetRandomPointInRadiusWithinPolygon(transform.position, _localRadius, polygonCenter, polygon, out result);
    }

    bool TryGetWeightedWanderPoint(out Vector3 result)
    {
        if (NPCBounds.Instance == null)
        {
            result = default;
            return false;
        }

        Vector2 playerDirection = GetPlayerDirection();

        Vector3[] candidates = new Vector3[_candidateCount];
        float[] weights = new float[_candidateCount];
        int validCount = 0;
        float totalWeight = 0f;

        for (int i = 0; i < _candidateCount; i++)
        {
            if (!NavMeshSpawner.TryGetRandomPointInPolygon(NPCBounds.Instance.Center, NPCBounds.Instance.Polygon, out Vector3 candidate))
                continue;

            float weight = ComputeWeight(candidate, playerDirection);
            candidates[validCount] = candidate;
            weights[validCount] = weight;
            totalWeight += weight;
            validCount++;
        }

        if (validCount == 0)
        {
            result = default;
            return false;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < validCount; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
            {
                result = candidates[i];
                return true;
            }
        }

        result = candidates[validCount - 1];
        return true;
    }

    Vector2 GetPlayerDirection()
    {
        if (_player == null)
            return Vector2.zero;

        Vector3 toPlayer = _player.position - transform.position;
        Vector2 direction = new Vector2(toPlayer.x, toPlayer.z);
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
    }

    float ComputeWeight(Vector3 candidate, Vector2 playerDirection)
    {
        if (playerDirection == Vector2.zero)
            return 1f;

        Vector3 toCandidate = candidate - transform.position;
        Vector2 candidateDirection = new Vector2(toCandidate.x, toCandidate.z);
        if (candidateDirection.sqrMagnitude < 0.0001f)
            return 1f;

        float alignment = Vector2.Dot(candidateDirection.normalized, playerDirection);
        float normalized = (alignment + 1f) * 0.5f;
        return Mathf.Pow(normalized, _playerBiasStrength) + 0.01f;
    }
}
