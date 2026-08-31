using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCQuestPoolManager : MonoBehaviour
{
    public static NPCQuestPoolManager Instance { get; private set; }

    // X: real, non-anomaly main-quest givers.
    [SerializeField] int _maxActiveQuestGivers = 4;
    // Z: total questable-looking NPCs for the day (X real + Y fake combined). Y isn't its own
    // field - it's whatever's left of Z after X is spent, shared between true anomalies and
    // unpicked-main-quest variants. Should be >= _maxActiveQuestGivers.
    [SerializeField] int _maxTotalQuestableNpcs = 6;
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

        // Provisional - HandleNewDayNextFrame re-evaluates every anomaly against the shared Z
        // budget a frame later and may clear this if it doesn't fit. Set here too so an anomaly
        // still shows something correct if that later pass never runs for some reason.
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

        FillEmptySlots(); // X real, non-anomaly main-quest givers

        // Everything past X shares one combined "fake" budget (Y) that, added to X, caps out at
        // _maxTotalQuestableNpcs (Z) questable-looking NPCs for the day. True anomalies get first
        // claim on that shared budget - the "find the anomaly" mechanic matters more than a
        // red-herring - then whatever's left goes to normal-pool NPCs sitting on an unpicked main
        // quest name (a real quest, just not one of today's 4). Both look identical to a real
        // quest-giver, so a lit marker is never proof an NPC matters.
        int questableCount = _activeQuestGivers.Count;

        // Quest names assigned to today's real givers that are ALSO flagged AllowsUncappedQuestSibling
        // (confiscation types only) - lets us recognize "the other spawned copy of a genuine
        // confiscation pair" apart from an incidental duplicate spawn. Fetch-item NPCs can share a
        // quest name too (NPCManager's weighted-random fill can spawn an extra copy of any
        // character on top of its guaranteed one), but that's an accident, not a deliberate pair,
        // so it must NOT get the uncapped-sibling treatment - only genuine pairs should ever
        // exceed Z, since that's the whole point of the confiscation puzzle (comparing both
        // against the notepad clue).
        HashSet<string> realPairQuestNames = new();
        foreach (GameObject giver in _activeQuestGivers)
        {
            if (giver != null && giver.TryGetComponent(out NPCQuestMarker giverMarker)
                && giverMarker.AllowsUncappedQuestSibling && !string.IsNullOrEmpty(giverMarker.QuestName))
                realPairQuestNames.Add(giverMarker.QuestName);
        }

        // Same explicit-refresh reasoning as the loop below - a freshly spawned anomaly's own
        // OnNewDayStarted subscription always misses the event that spawned it.
        foreach (GameObject anomalyNpc in _anomalyNpcs)
        {
            if (anomalyNpc == null)
                continue;

            bool getsMarker = questableCount < _maxTotalQuestableNpcs;
            if (getsMarker)
                questableCount++;

            if (anomalyNpc.TryGetComponent(out NPCQuestGiverState anomalyState))
            {
                if (getsMarker)
                    anomalyState.SetQuestGiver(null, true);
                else
                    anomalyState.ClearQuestGiver();
            }

            if (anomalyNpc.TryGetComponent(out NPCQuestMarker anomalyMarker))
                anomalyMarker.RefreshIndicator();
        }

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

            bool hasQuestName = marker != null && !marker.IsForcedNonQuestable && !string.IsNullOrEmpty(marker.QuestName);
            bool isRealQuestSibling = hasQuestName && marker.AllowsUncappedQuestSibling && realPairQuestNames.Contains(marker.QuestName);
            bool hasFakeQuest = hasQuestName && (isRealQuestSibling || questableCount < _maxTotalQuestableNpcs);

            if (hasFakeQuest)
            {
                // Only counts against the shared budget if it's a generic decoy - a real quest's
                // sibling copy is uncapped, it doesn't consume (or need) any of Z.
                if (!isRealQuestSibling)
                    questableCount++;

                state?.SetQuestGiver(marker.QuestName, true);
            }
            else
            {
                state?.ClearQuestGiver();
                if (npc.TryGetComponent(out NPCIdentity identity))
                    identity.SetGenericConversation(_nonQuestableConversation);
            }

            marker?.RefreshIndicator();
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
        // Prefers a non-anomaly candidate when more than one spawned copy shares this quest name
        // (a confiscation type's two variants, each with independently-rolled anomaly status) -
        // otherwise the quest could get permanently assigned to whichever copy happens to be the
        // anomaly, which can never resolve it (confiscating it triggers the penalty instead).
        // Still falls back to an anomaly candidate rather than dropping the quest outright, for
        // the rare case every matching copy rolled anomaly.
        GameObject anomalyFallback = null;

        foreach (GameObject candidate in _normalPool)
        {
            if (candidate == null || _activeQuestGivers.Contains(candidate))
                continue;

            if (!candidate.TryGetComponent(out NPCQuestMarker marker) || marker.IsForcedNonQuestable || marker.QuestName != questName)
                continue;

            bool isAnomaly = candidate.TryGetComponent(out NPCIdentity identity) && identity.IsAnomaly;
            if (!isAnomaly)
            {
                npc = candidate;
                return true;
            }

            if (anomalyFallback == null)
                anomalyFallback = candidate;
        }

        if (anomalyFallback != null)
        {
            npc = anomalyFallback;
            return true;
        }

        npc = null;
        return false;
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
