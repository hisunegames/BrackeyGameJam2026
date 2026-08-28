using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCQuestMarker : MonoBehaviour
{
    [SerializeField] string _questName;
    [SerializeField] AnomalyMarker _marker;

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationEnd(Transform actor)
    {
        if (string.IsNullOrEmpty(_questName) || _marker == null)
            return;

        if (QuestLog.IsQuestInStateMask(_questName, QuestState.Active))
            _marker.SetQuestMarked(true);
    }
}
