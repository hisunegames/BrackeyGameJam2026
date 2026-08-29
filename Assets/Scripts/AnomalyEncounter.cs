using PixelCrushers.DialogueSystem;
using UnityEngine;

public class AnomalyEncounter : MonoBehaviour
{
    [SerializeField] string _requiredQuestName;

    bool _hasTriggeredPenalty;

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationEnd(Transform actor)
    {
        if (_hasTriggeredPenalty)
            return;

        if (!string.IsNullOrEmpty(_requiredQuestName) && !QuestLog.IsQuestInStateMask(_requiredQuestName, QuestState.Success))
            return;

        _hasTriggeredPenalty = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Penalty);
    }
}
