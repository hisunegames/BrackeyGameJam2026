using System.Collections.Generic;
using UnityEngine;

public class NPCQuestPoolManager : MonoBehaviour
{
    public static NPCQuestPoolManager Instance { get; private set; }

    [SerializeField] int _maxActiveQuestGivers = 4;

    readonly List<GameObject> _normalPool = new();
    readonly List<GameObject> _activeQuestGivers = new();
    readonly Queue<string> _pendingQuestNames = new();
    GameObject _anomalyNpc;

    public IReadOnlyList<GameObject> ActiveQuestGivers => _activeQuestGivers;
    public GameObject AnomalyNpc => _anomalyNpc;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ShiftManager.OnNewDayStarted += HandleNewDay;
    }

    void OnDisable()
    {
        ShiftManager.OnNewDayStarted -= HandleNewDay;
    }

    // Called by NPCManager right after spawning each NPC.
    public void RegisterNormalNpc(GameObject npc)
    {
        _normalPool.Add(npc);
    }

    public void RegisterAnomalyNpc(GameObject npc)
    {
        _anomalyNpc = npc;

        // The anomaly always shows a quest icon too, as a decoy, regardless of the 4-slot limit.
        if (npc.TryGetComponent(out NPCQuestGiverState state))
            state.SetQuestGiver(null, true);
    }

    void HandleNewDay(int day)
    {
        _normalPool.RemoveAll(npc => npc == null);
        _activeQuestGivers.Clear();
        _pendingQuestNames.Clear();

        if (ShiftManager.Instance != null)
        {
            foreach (string questName in ShiftManager.Instance.MainQuestNames)
            {
                if (!string.IsNullOrEmpty(questName))
                    _pendingQuestNames.Enqueue(questName);
            }
        }

        FillEmptySlots();

        if (_anomalyNpc != null && _anomalyNpc.TryGetComponent(out NPCQuestGiverState anomalyState))
            anomalyState.SetQuestGiver(null, true);
    }

    void FillEmptySlots()
    {
        while (_activeQuestGivers.Count < _maxActiveQuestGivers && _pendingQuestNames.Count > 0)
        {
            if (!TryPickUnassignedNpc(out GameObject npc))
                break;

            string questName = _pendingQuestNames.Dequeue();
            _activeQuestGivers.Add(npc);

            if (npc.TryGetComponent(out NPCQuestGiverState state))
                state.SetQuestGiver(questName, false);
        }
    }

    bool TryPickUnassignedNpc(out GameObject npc)
    {
        HashSet<string> activeCharacterIds = new();
        foreach (GameObject active in _activeQuestGivers)
        {
            if (active != null && active.TryGetComponent(out NPCIdentity activeIdentity) && !string.IsNullOrEmpty(activeIdentity.CharacterId))
                activeCharacterIds.Add(activeIdentity.CharacterId);
        }

        List<GameObject> candidates = new();
        foreach (GameObject candidate in _normalPool)
        {
            if (candidate == null || _activeQuestGivers.Contains(candidate))
                continue;

            // Skip duplicate spawns of an NPC that's already a quest giver - two active copies
            // of the "same" character showing different note variants is confusing, not a decoy.
            if (candidate.TryGetComponent(out NPCIdentity candidateIdentity)
                && !string.IsNullOrEmpty(candidateIdentity.CharacterId)
                && activeCharacterIds.Contains(candidateIdentity.CharacterId))
                continue;

            candidates.Add(candidate);
        }

        if (candidates.Count == 0)
        {
            npc = null;
            return false;
        }

        npc = candidates[Random.Range(0, candidates.Count)];
        return true;
    }

    // Called by NPCQuestGiverState.ResolveQuest() once a quest has been accepted or rejected -
    // frees this NPC's slot and pulls in a new NPC + the next pending quest to replace it.
    public void NotifyQuestResolved(GameObject npc)
    {
        if (!_activeQuestGivers.Remove(npc))
            return;

        if (npc.TryGetComponent(out NPCQuestGiverState state))
            state.ClearQuestGiver();

        FillEmptySlots();
    }

    // Debug only - fills the questable slots from whatever NPCs are available using placeholder
    // quest names, bypassing ShiftManager.MainQuestNames entirely. For testing the pool/notepad
    // pipeline before real Dialogue System quests exist on the questable NPCs.
    public void DebugFillWithPlaceholders()
    {
        _normalPool.RemoveAll(npc => npc == null);

        int placeholderIndex = _activeQuestGivers.Count;
        while (_activeQuestGivers.Count < _maxActiveQuestGivers)
        {
            if (!TryPickUnassignedNpc(out GameObject npc))
                break;

            placeholderIndex++;
            _activeQuestGivers.Add(npc);

            if (npc.TryGetComponent(out NPCQuestGiverState state))
                state.SetQuestGiver($"DebugQuest{placeholderIndex}", false);
        }
    }
}
