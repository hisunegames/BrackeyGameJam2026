using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCQuestMarker : MonoBehaviour
{
    [SerializeField] string _questName;
    [SerializeField] AnomalyMarker _marker;
    [SerializeField] NPCQuestGiverState _questGiverState;

    [Header("World-Space Sprite Swap")]
    [SerializeField] SpriteRenderer _indicatorSprite;
    [SerializeField] Sprite _questAvailableSprite;
    [SerializeField] Sprite _questAcceptedSprite;
    [SerializeField] Sprite _defaultSprite;

    // For NPCs whose quest item/sprite isn't ready yet - keeps them out of the quest-giver
    // pool entirely even if _questName is filled in, without needing to clear it.
    [SerializeField] bool _forceNonQuestable;

    public string QuestName => _questName;
    public bool IsForcedNonQuestable => _forceNonQuestable;

    QuestMarkerStateBridge _questStateBridge;

    void Awake()
    {
        if (_questGiverState == null)
            _questGiverState = GetComponent<NPCQuestGiverState>();
    }

    void OnEnable()
    {
        ShiftManager.OnNewDayStarted += HandleNewDayStarted;

        // OnConversationEnd only fires for the NPC actually being talked to - a duplicate spawn
        // sharing this same static QuestName (e.g. an anomaly decoy standing in for the real
        // quest-giver) would otherwise never learn the quest was accepted/completed through the
        // OTHER instance's conversation. Bridges Dialogue System's own global quest-state-change
        // notification (QuestStateDispatcher) to RefreshIndicator() instead. Added at runtime
        // rather than baked into every prefab, so no per-NPC setup/data migration is needed.
        if (_questStateBridge == null)
            _questStateBridge = gameObject.AddComponent<QuestMarkerStateBridge>();
        _questStateBridge.Init(this, _questName);
    }

    void OnDisable()
    {
        ShiftManager.OnNewDayStarted -= HandleNewDayStarted;
    }

    // Shows the marker right when the day starts if this NPC's quest is one of today's picks.
    // Kept for NPCs that persist across days without being respawned - freshly Instantiate()'d
    // NPCs subscribe to OnNewDayStarted too late to catch that same dispatch (see RefreshIndicator).
    void HandleNewDayStarted(int day)
    {
        RefreshIndicator();
    }

    // Called explicitly by NPCQuestPoolManager after it finishes matching each day, since a
    // freshly spawned NPC's own OnNewDayStarted subscription always misses the event that
    // spawned it (added mid-dispatch, doesn't apply to the invocation already in progress).
    public void RefreshIndicator()
    {
        // Gate on IsQuestGiver (set by NPCQuestPoolManager.FillEmptySlots, which runs before this
        // is called) rather than independently re-checking "is my quest name one of today's picks" -
        // that check doesn't know whether THIS specific instance was the one actually selected.
        // Two NPCs sharing the same quest name (a duplicate variant spawn) would otherwise both
        // show "available" even though only one of them actually got the quest-giver conversation.
        if (_forceNonQuestable || string.IsNullOrEmpty(_questName) || _questGiverState == null || !_questGiverState.IsQuestGiver)
        {
            SetIndicatorState(QuestMarkerState.None);
            return;
        }

        // Already accepted (e.g. NPC respawned mid-conversation-cycle, or state persisted) takes
        // priority over "available".
        if (QuestLog.IsQuestInStateMask(_questName, QuestState.Active))
        {
            SetIndicatorState(QuestMarkerState.Accepted);
            return;
        }

        if (QuestLog.IsQuestInStateMask(_questName, QuestState.Success))
        {
            SetIndicatorState(QuestMarkerState.None);
            return;
        }

        SetIndicatorState(QuestMarkerState.Available);
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationEnd(Transform actor)
    {
        if (string.IsNullOrEmpty(_questName))
            return;

        // Now shows a distinct "accepted" state instead of just turning off - lets the player
        // tell "available" and "already talked to, quest in progress" apart at a glance.
        if (QuestLog.IsQuestInStateMask(_questName, QuestState.Active))
            SetIndicatorState(QuestMarkerState.Accepted);

        if (QuestLog.IsQuestInStateMask(_questName, QuestState.Success))
            SetIndicatorState(QuestMarkerState.None);
    }

    void SetIndicatorState(QuestMarkerState state)
    {
        if (_marker != null)
            _marker.SetQuestState(state);

        if (_indicatorSprite != null)
        {
            _indicatorSprite.sprite = state switch
            {
                QuestMarkerState.Available => _questAvailableSprite,
                QuestMarkerState.Accepted => _questAcceptedSprite,
                _ => _defaultSprite
            };
        }
    }
}

// Minimal bridge from Dialogue System's QuestStateListener/QuestStateDispatcher to
// NPCQuestMarker.RefreshIndicator() - see NPCQuestMarker.OnEnable for why this is needed.
// Ignores the base class's own indicator-level machinery (questStateIndicatorLevels is left
// empty) since RefreshIndicator() already does the real state check.
class QuestMarkerStateBridge : QuestStateListener
{
    NPCQuestMarker _marker;

    public void Init(NPCQuestMarker marker, string questNameToWatch)
    {
        _marker = marker;
        questName = questNameToWatch;
    }

    public override void OnChange()
    {
        if (_marker != null)
            _marker.RefreshIndicator();
    }
}
