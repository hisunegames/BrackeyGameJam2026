using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    static int _nextId;

    [SerializeField] string _uniqueId;
    [SerializeField] NoteData _associatedNote;

    public string UniqueId => _uniqueId;
    public bool IsAnomaly { get; private set; }
    public NoteData AssociatedNote => _associatedNote;

    void Awake()
    {
        _uniqueId = $"NPC_{_nextId++}";
    }

    public void SetIsAnomaly(bool isAnomaly)
    {
        IsAnomaly = isAnomaly;
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        DialogueLua.SetVariable("NPC.CurrentInstanceID", _uniqueId);
    }
}
