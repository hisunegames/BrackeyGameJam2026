using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Alerted,
    Chase
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyLineOfSight))]
public class EnemyChaser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] EnemyLineOfSight _lineOfSight;
    [SerializeField] RandomMovement _wanderBehaviour;

    [Header("Chase Settings")]
    [SerializeField] float _chaseSpeed = 6f;
    [SerializeField] float _chaseAcceleration = 2f;
    [SerializeField] float _loseSightDelay = 3f;

    [Header("Detection Wind-Up")]
    [SerializeField] float _windUpDuration = 1.5f;
    [SerializeField] float _presenceWindUpDuration = 3f;
    [SerializeField] float _turnSpeed = 240f;

    [Header("Animation")]
    [SerializeField] Animator _animator;
    [SerializeField] string _isAlertedParam = "IsAlerted";
    [SerializeField] string _isChasingParam = "IsChasing";
    [SerializeField] string _screamTrigger = "Scream";

    [Header("Audio")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _screamSound;

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    NavMeshAgent _agent;
    Transform _target;
    float _normalSpeed;
    float _windUpTimer;
    float _loseSightTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _normalSpeed = _agent.speed;

        if (_lineOfSight == null)
            _lineOfSight = GetComponent<EnemyLineOfSight>();

        if (_wanderBehaviour == null)
            _wanderBehaviour = GetComponent<RandomMovement>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _target = player.transform;
    }

    void Update()
    {
        if (_target == null)
            return;

        switch (CurrentState)
        {
            case EnemyState.Idle:
                if (_lineOfSight.HasDetectedPlayer)
                    EnterAlerted();
                break;

            case EnemyState.Alerted:
                if (!_lineOfSight.HasDetectedPlayer)
                {
                    EnterIdle();
                    break;
                }

                if (_lineOfSight.CanSeePlayer)
                    FaceTarget();

                _windUpTimer -= Time.deltaTime;
                if (_windUpTimer <= 0f)
                    EnterChase();
                break;

            case EnemyState.Chase:
                // Ramps up toward chase speed rather than snapping to it - stops ramping (and
                // gets reset to base speed) the moment interest is lost, via EnterIdle().
                _agent.speed = Mathf.MoveTowards(_agent.speed, _chaseSpeed, _chaseAcceleration * Time.deltaTime);

                if (_lineOfSight.HasDetectedPlayer)
                {
                    _loseSightTimer = _loseSightDelay;
                    _agent.SetDestination(_target.position);
                }
                else
                {
                    _loseSightTimer -= Time.deltaTime;
                    if (_loseSightTimer <= 0f)
                        EnterIdle();
                }
                break;
        }
    }

    public void ForceChase()
    {
        EnterChase();
    }

    void FaceTarget()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }

    void EnterAlerted()
    {
        CurrentState = EnemyState.Alerted;
        _windUpTimer = _lineOfSight.CanSeePlayer ? _windUpDuration : _presenceWindUpDuration;
        _agent.isStopped = true;

        if (_wanderBehaviour != null)
            _wanderBehaviour.enabled = false;

        if (_animator != null)
            _animator.SetBool(_isAlertedParam, true);
    }

    void EnterChase()
    {
        CurrentState = EnemyState.Chase;
        _loseSightTimer = _loseSightDelay;
        _agent.isStopped = false;
        // Speed isn't snapped here - it ramps up toward _chaseSpeed frame by frame in Update()
        // instead, starting from whatever it's currently at (base speed, since nothing else
        // touches it between EnterIdle() and here).
        _agent.SetDestination(_target.position);

        if (_animator != null)
        {
            _animator.SetBool(_isAlertedParam, false);
            _animator.SetBool(_isChasingParam, true);
            _animator.SetTrigger(_screamTrigger);
        }

        if (_audioSource != null && _screamSound != null)
            _audioSource.PlayOneShot(_screamSound);

        if (GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Chase);
    }

    void EnterIdle()
    {
        bool wasChasing = CurrentState == EnemyState.Chase;

        CurrentState = EnemyState.Idle;
        _agent.isStopped = false;
        _agent.speed = _normalSpeed;

        if (_wanderBehaviour != null)
            _wanderBehaviour.enabled = true;
        else
            _agent.ResetPath();

        if (_animator != null)
        {
            _animator.SetBool(_isAlertedParam, false);
            _animator.SetBool(_isChasingParam, false);
        }

        if (wasChasing && GameManager.Instance != null && GameManager.Instance.CurrentSequence == GameplaySequence.Chase)
        {
            // If a penalty encounter is still unresolved (the hide timer hasn't run out), resume hiding instead of
            // fully ending it - escaping a chase shouldn't be a free pass on the rest of the timer.
            bool resumePenalty = PenaltyManager.Instance != null && PenaltyManager.Instance.IsEncounterActive;
            GameManager.Instance.SetSequence(resumePenalty ? GameplaySequence.Penalty : GameplaySequence.Normal);
        }
    }
}
