using System;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyManager : MonoBehaviour
{
    [Header("Monster Pool")]
    [SerializeField] GameObject[] _monsterVariants;

    [Header("Spawn Settings")]
    [SerializeField] Transform _player;
    [SerializeField] Vector3 _spawnOffset;
    [SerializeField] Vector2[] _spawnPolygon;

    Vector3 SpawnCenter => (_player != null ? _player.position : transform.position) + _spawnOffset;

    [Header("Timer")]
    [SerializeField] float _penaltyDuration = 30f;

    public float RemainingTime { get; private set; }
    public static event Action<float> OnPenaltyTimerChanged;

    public static PenaltyManager Instance { get; private set; }
    public bool IsEncounterActive { get; private set; }

    readonly List<GameObject> _spawnedMonsters = new();
    bool _isPenaltyActive;

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
        switch (sequence)
        {
            case GameplaySequence.Penalty:
                if (IsEncounterActive)
                    _isPenaltyActive = true; // resuming the hide timer after escaping a chase; don't respawn or reset it
                else
                    StartPenalty();
                break;

            case GameplaySequence.Normal:
                // Encounter fully resolved: either the hide timer expired, or EnemyChaser decided (via IsEncounterActive)
                // there was nothing left to resume and went straight to Normal itself.
                IsEncounterActive = false;
                _isPenaltyActive = false;
                DespawnMonsters();
                break;

            default:
                // e.g. caught -> Chase: pause the hide-timer countdown, but leave the monster and encounter alone.
                _isPenaltyActive = false;
                break;
        }
    }

    void StartPenalty()
    {
        IsEncounterActive = true;
        _isPenaltyActive = true;
        RemainingTime = _penaltyDuration;
        OnPenaltyTimerChanged?.Invoke(RemainingTime);
        SpawnMonster();
    }

    void Update()
    {
        if (!_isPenaltyActive)
            return;

        RemainingTime -= Time.deltaTime;
        OnPenaltyTimerChanged?.Invoke(Mathf.Max(0f, RemainingTime));

        if (RemainingTime <= 0f && GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Normal);
    }

    void SpawnMonster()
    {
        if (_monsterVariants.Length == 0 || _player == null)
            return;

        GameObject prefab = _monsterVariants[UnityEngine.Random.Range(0, _monsterVariants.Length)];
        if (!NavMeshSpawner.TryGetRandomPointInPolygon(SpawnCenter, _spawnPolygon, out Vector3 point))
            return;

        GameObject monster = Instantiate(prefab, point, Quaternion.identity);
        _spawnedMonsters.Add(monster);

        if (monster.TryGetComponent(out EnemyChaser chaser))
            chaser.ForceChase();
    }

    void DespawnMonsters()
    {
        foreach (GameObject monster in _spawnedMonsters)
        {
            if (monster != null)
                Destroy(monster);
        }
        _spawnedMonsters.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (_spawnPolygon == null || _spawnPolygon.Length < 2)
            return;

        Vector3 center = SpawnCenter;

        Gizmos.color = Color.red;
        for (int i = 0; i < _spawnPolygon.Length; i++)
        {
            Vector2 a = _spawnPolygon[i];
            Vector2 b = _spawnPolygon[(i + 1) % _spawnPolygon.Length];
            Gizmos.DrawLine(center + new Vector3(a.x, 0f, a.y), center + new Vector3(b.x, 0f, b.y));
        }
    }
}
