using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class AnomalyEncounter : MonoBehaviour
{
    [SerializeField] string _requiredQuestName;
    [SerializeField] NPCIdentity _npcIdentity;

    bool _hasTriggeredPenalty;

    public static event Action OnAnomalyQuestTriggeredPenalty;

    void Awake()
    {
        if (_npcIdentity == null)
            _npcIdentity = GetComponent<NPCIdentity>();
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationEnd(Transform actor)
    {
        if (_hasTriggeredPenalty)
            return;

        // This component lives on every NPC (added to the shared NPC.prefab root) but only the
        // fetch-item characters have _requiredQuestName actually set - an empty name here means
        // "not configured as an anomaly trap," not "no gate, trigger on anything."
        if (string.IsNullOrEmpty(_requiredQuestName))
            return;

        // The quest name is baked per character, not per instance - the real (non-anomaly)
        // Billionaire and an anomaly that happens to also look like Billionaire share the same
        // _requiredQuestName. Without this, finishing the anomaly's fake quest anywhere in the
        // mall would trip this on the real NPC too, since QuestLog state alone can't tell them
        // apart.
        if (_npcIdentity == null || !_npcIdentity.IsAnomaly)
            return;

        if (!QuestLog.IsQuestInStateMask(_requiredQuestName, QuestState.Success))
            return;

        _hasTriggeredPenalty = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Penalty);

        OnAnomalyQuestTriggeredPenalty?.Invoke();

        // Remove it outright rather than just leaving _hasTriggeredPenalty to silently no-op
        // future conversations - a lingering NPC that's already done its job is easy to
        // accidentally talk to again and confusing when nothing happens.
        Destroy(gameObject);
    }
}
