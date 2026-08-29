using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    static int _nextId;

    [SerializeField] string _uniqueId;

    public string UniqueId => _uniqueId;

    void Awake()
    {
        _uniqueId = $"NPC_{_nextId++}";
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        DialogueLua.SetVariable("NPC.CurrentInstanceID", _uniqueId);
    }
}
