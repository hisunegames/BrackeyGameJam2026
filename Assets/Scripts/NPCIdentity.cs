using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    static int _nextId;

    [SerializeField] string _uniqueId;
    [SerializeField] NoteData[] _noteVariants;
    [SerializeField] DialogueSystemTrigger _dialogueTrigger;

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
        IsAnomaly = isAnomaly;
        ResolveNoteVariant();
    }

    void ResolveNoteVariant()
    {
        // The anomaly is identified by NOT matching any posted note, not by having a "wrong" one.
        if (IsAnomaly || _noteVariants == null || _noteVariants.Length == 0)
        {
            AssociatedNote = null;
            return;
        }

        AssociatedNote = _noteVariants[Random.Range(0, _noteVariants.Length)];

        // Keeps this NPC's actual dialogue in sync with whichever note variant was picked.
        if (!string.IsNullOrEmpty(AssociatedNote.ConversationName) && _dialogueTrigger != null)
            _dialogueTrigger.conversation = AssociatedNote.ConversationName;
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        DialogueLua.SetVariable("NPC.CurrentInstanceID", _uniqueId);
    }
}
