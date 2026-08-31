using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PenaltyManager : MonoBehaviour
{
    [Header("Monster Pool")]
    [SerializeField] GameObject[] _monsterVariants;

    [Header("Spawn Settings")]
    [SerializeField] Transform _player;
    [SerializeField] Vector3 _spawnOffset;
    [SerializeField] Vector2[] _spawnPolygon;
    // _spawnPolygon currently duplicates NPCBounds' own (whole-mall-sized) shape rather than a
    // small area local to the player, so intersecting it against NPCBounds.Polygon (anchored at
    // NPCBounds' own fixed position, not the player) can come up empty depending on where the
    // player actually is - e.g. anywhere far from wherever _spawnOffset happens to have been
    // tuned against. Falls back to a plain radius-based NavMesh sample around SpawnCenter so a
    // misconfigured/too-small polygon intersection never silently skips the spawn entirely.
    [SerializeField] float _fallbackSpawnRadius = 15f;

    Vector3 SpawnCenter => (_player != null ? _player.position : transform.position) + _spawnOffset;

    [Header("Timer")]
    [SerializeField] float _penaltyDuration = 30f;

    [Header("Despawn")]
    [SerializeField] float _monsterFadeDuration = 1f;

    public float RemainingTime { get; private set; }
    public static event Action<float> OnPenaltyTimerChanged;

    public static PenaltyManager Instance { get; private set; }
    public bool IsEncounterActive { get; private set; }
    public bool IsElectricBoxFixed { get; private set; }

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
        IsElectricBoxFixed = false;
        _isPenaltyActive = true;
        RemainingTime = _penaltyDuration;
        OnPenaltyTimerChanged?.Invoke(RemainingTime);
        SpawnMonster();
    }

    public void NotifyElectricBoxFixed()
    {
        if (IsElectricBoxFixed)
            return;

        IsElectricBoxFixed = true;
        OnPenaltyTimerChanged?.Invoke(RemainingTime);
    }

    void Update()
    {
        if (!_isPenaltyActive || !IsElectricBoxFixed)
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

        Vector3 boundsCenter = NPCBounds.Instance != null ? NPCBounds.Instance.Center : Vector3.zero;
        Vector2[] boundsPolygon = NPCBounds.Instance != null ? NPCBounds.Instance.Polygon : null;

        bool foundPoint = NavMeshSpawner.TryGetRandomPointInPolygon(SpawnCenter, _spawnPolygon, boundsCenter, boundsPolygon, out Vector3 point);
        if (!foundPoint)
            foundPoint = NavMeshSpawner.TryGetRandomPoint(SpawnCenter, _fallbackSpawnRadius, out point);

        if (!foundPoint)
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
            if (monster == null)
                continue;

            if (monster.TryGetComponent(out BillboardSprite billboard))
                billboard.enabled = false;

            foreach (SpriteRenderer spriteRenderer in monster.GetComponentsInChildren<SpriteRenderer>())
                spriteRenderer.DOFade(0f, _monsterFadeDuration);

            Destroy(monster, _monsterFadeDuration);
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
