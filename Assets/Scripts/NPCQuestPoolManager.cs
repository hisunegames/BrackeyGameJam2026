using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCQuestPoolManager : MonoBehaviour
{
    public static NPCQuestPoolManager Instance { get; private set; }

    [SerializeField] int _maxActiveQuestGivers = 4;
    [ConversationPopup]
    [SerializeField] string _nonQuestableConversation = "NonQuestableNPC";

    readonly List<GameObject> _normalPool = new();
    readonly List<GameObject> _activeQuestGivers = new();
    readonly Queue<string> _pendingQuestNames = new();
    readonly List<GameObject> _anomalyNpcs = new();

    public IReadOnlyList<GameObject> ActiveQuestGivers => _activeQuestGivers;
    public IReadOnlyList<GameObject> AnomalyNpcs => _anomalyNpcs;

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
        // A list, not a single field - NPCManager can spawn more than one anomaly-flavored NPC a
        // day (the guaranteed one, plus any that roll isAnomaly=true from the random fill chance).
        _anomalyNpcs.Add(npc);

        // Every anomaly always shows a quest icon too, as a decoy, regardless of the 4-slot limit.
        if (npc.TryGetComponent(out NPCQuestGiverState state))
            state.SetQuestGiver(null, true);
    }

    void HandleNewDay(int day)
    {
        // NPCManager and ConfiscationNpcSpawner also react to this same event to spawn and
        // register the day's NPCs - if this handler happens to run first (subscriber order isn't
        // guaranteed), _normalPool would still be empty/stale and every match would fail. Defer
        // by a frame so registration has definitely happened first (same fix as NotepadPodiumSpawner).
        StartCoroutine(HandleNewDayNextFrame(day));
    }

    IEnumerator HandleNewDayNextFrame(int day)
    {
        yield return null;

        _normalPool.RemoveAll(npc => npc == null);
        _anomalyNpcs.RemoveAll(npc => npc == null);
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

        // Anyone not selected as one of today's real main-objective quest-givers is nonquestable -
        // no fake decoy markers for the main-quest pool, so a lit-up marker always means a real
        // objective and duplicate spawns of the same character never show more than one marker
        // between them. The anomaly loop below is a separate, always-on decoy mechanic and isn't
        // affected by this.
        foreach (GameObject npc in _normalPool)
        {
            if (npc == null)
                continue;

            npc.TryGetComponent(out NPCQuestMarker marker);
            npc.TryGetComponent(out NPCQuestGiverState state);

            if (_activeQuestGivers.Contains(npc))
            {
                marker?.RefreshIndicator();
                continue;
            }

            state?.ClearQuestGiver();
            if (npc.TryGetComponent(out NPCIdentity identity))
                identity.SetGenericConversation(_nonQuestableConversation);

            marker?.RefreshIndicator();
        }

        // Same explicit-refresh reasoning as above - a freshly spawned anomaly's own
        // OnNewDayStarted subscription always misses the event that spawned it.
        foreach (GameObject anomalyNpc in _anomalyNpcs)
        {
            if (anomalyNpc == null)
                continue;

            if (anomalyNpc.TryGetComponent(out NPCQuestGiverState anomalyState))
                anomalyState.SetQuestGiver(null, true);

            if (anomalyNpc.TryGetComponent(out NPCQuestMarker anomalyMarker))
                anomalyMarker.RefreshIndicator();
        }
    }

    void FillEmptySlots()
    {
        // Each pending quest belongs to a specific NPC (whatever they're hand-authored with on
        // NPCQuestMarker) - match by identity instead of handing a random NPC a random quest.
        int pendingCount = _pendingQuestNames.Count;
        for (int i = 0; i < pendingCount && _activeQuestGivers.Count < _maxActiveQuestGivers; i++)
        {
            string questName = _pendingQuestNames.Dequeue();

            if (!TryFindNpcForQuest(questName, out GameObject npc))
                continue; // nobody currently spawned owns this quest today - drop it

            _activeQuestGivers.Add(npc);

            if (npc.TryGetComponent(out NPCQuestGiverState state))
                state.SetQuestGiver(questName, false);
        }
    }

    bool TryFindNpcForQuest(string questName, out GameObject npc)
    {
        foreach (GameObject candidate in _normalPool)
        {
            if (candidate == null || _activeQuestGivers.Contains(candidate))
                continue;

            if (candidate.TryGetComponent(out NPCQuestMarker marker) && !marker.IsForcedNonQuestable && marker.QuestName == questName)
            {
                npc = candidate;
                return true;
            }
        }

        npc = null;
        return false;
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
            if (!DebugTryPickUnassignedNpc(out GameObject npc))
                break;

            placeholderIndex++;
            _activeQuestGivers.Add(npc);

            if (npc.TryGetComponent(out NPCQuestGiverState state))
                state.SetQuestGiver($"DebugQuest{placeholderIndex}", false);
        }
    }

    // Debug only - the live FillEmptySlots() path matches NPCs to quests by identity via
    // TryFindNpcForQuest instead, since placeholder quest names here don't belong to any NPC's
    // own NPCQuestMarker. Picks any unassigned NPC at random, deduped by CharacterId so two
    // spawned copies of the same character don't both get pulled in.
    bool DebugTryPickUnassignedNpc(out GameObject npc)
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
}
