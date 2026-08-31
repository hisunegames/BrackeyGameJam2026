using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    static int _nextId;

    [SerializeField] string _uniqueId;
    [SerializeField] NoteData[] _noteVariants;
    [ConversationPopup]
    [SerializeField] string[] _anomalyConversations;
    [SerializeField] DialogueSystemTrigger _dialogueTrigger;

    // For NPCs whose anomaly status shouldn't be its own independent coin flip - instead, a
    // random note variant is picked first, and that note's own IndicatesAnomaly flag decides
    // IsAnomaly. Lets a note's content (a keyword) be the anomaly tell, instead of the anomaly
    // withholding its note entirely like the normal flow does. The isAnomaly passed into
    // SetIsAnomaly is ignored when this is on - the spawner's pre-roll doesn't apply.
    [SerializeField] bool _anomalyDeterminedByNoteVariant;

    [Header("Debug")]
    // Read-only in the Inspector - mirrors IsAnomaly so you can check a live-spawned instance's
    // anomaly status at a glance without needing to inspect it through a script window.
    [SerializeField] bool _debugIsAnomaly;

    public string UniqueId => _uniqueId;
    public bool IsAnomaly { get; private set; }

    // Which prefab/character this instance was spawned from (set by the spawner) - lets pool
    // managers tell "two copies of the same NPC" apart from "two different NPCs", since
    // UniqueId is per-instance and can't do that on its own.
    public string CharacterId { get; private set; }

    public void SetCharacterId(string characterId)
    {
        CharacterId = characterId;
    }

    // Called by NPCQuestPoolManager for any NPC that isn't one of today's active quest-givers -
    // overrides whatever conversation ResolveNoteVariant picked, since they have nothing to offer.
    public void SetGenericConversation(string conversationName)
    {
        if (_dialogueTrigger != null && !string.IsNullOrEmpty(conversationName))
            _dialogueTrigger.conversation = conversationName;
    }

    // Resolved once (in SetIsAnomaly). Only ever set for the true/normal NPC - the anomaly gets
    // no note at all, so it can never collide with (or contradict) whichever note gets posted.
    public NoteData AssociatedNote { get; private set; }

    void Awake()
    {
        _uniqueId = $"NPC_{_nextId++}";

        if (_dialogueTrigger == null)
            _dialogueTrigger = GetComponent<DialogueSystemTrigger>();
    }

    public void SetIsAnomaly(bool isAnomaly)
    {
        if (_anomalyDeterminedByNoteVariant)
        {
            ResolveNoteVariantAndAnomaly();
            return;
        }

        IsAnomaly = isAnomaly;
        _debugIsAnomaly = isAnomaly;
        ResolveNoteVariant();
    }

    // Picks a random note variant first, then derives IsAnomaly from that specific note instead
    // of a value handed in from outside. Every variant gets a real note either way - there's no
    // "no note = anomaly" tell here, the note's own content is the tell.
    void ResolveNoteVariantAndAnomaly()
    {
        NoteData note = PickAndApplyRandomNote();
        IsAnomaly = note != null && note.IndicatesAnomaly;
        _debugIsAnomaly = IsAnomaly;
    }

    void ResolveNoteVariant()
    {
        if (IsAnomaly)
        {
            // Still no note - identified by absence, not by having a "wrong" one. But its own
            // dialogue can pick from a separate pool written to actually give the game away.
            AssociatedNote = null;

            if (_anomalyConversations != null && _anomalyConversations.Length > 0 && _dialogueTrigger != null)
            {
                string conversation = _anomalyConversations[Random.Range(0, _anomalyConversations.Length)];
                if (!string.IsNullOrEmpty(conversation))
                    _dialogueTrigger.conversation = conversation;
            }

            return;
        }

        PickAndApplyRandomNote();
    }

    // Picks a random note variant, assigns it to AssociatedNote, and syncs the dialogue trigger's
    // conversation from it so what the player reads matches what the NPC actually says. Returns
    // null (leaving AssociatedNote null too) if there are no variants to pick from.
    NoteData PickAndApplyRandomNote()
    {
        if (_noteVariants == null || _noteVariants.Length == 0)
        {
            AssociatedNote = null;
            return null;
        }

        AssociatedNote = _noteVariants[Random.Range(0, _noteVariants.Length)];

        if (!string.IsNullOrEmpty(AssociatedNote.ConversationName) && _dialogueTrigger != null)
            _dialogueTrigger.conversation = AssociatedNote.ConversationName;

        return AssociatedNote;
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        DialogueLua.SetVariable("NPC.CurrentInstanceID", _uniqueId);
    }
}
