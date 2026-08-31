using UnityEngine;

// Accumulates run-wide stats for the Game Over screen. Lives for the whole play session,
// not reset between days - only makes sense to reset on a fresh run (main menu -> start).
public class GameStatsTracker : MonoBehaviour
{
    public static GameStatsTracker Instance { get; private set; }

    public int EnemyTriggerCount { get; private set; }
    public int AnomalyQuestTriggerCount { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnSequenceChanged += HandleSequenceChanged;
        AnomalyEncounter.OnAnomalyQuestTriggeredPenalty += HandleAnomalyQuestTriggered;
    }

    void OnDisable()
    {
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
        AnomalyEncounter.OnAnomalyQuestTriggeredPenalty -= HandleAnomalyQuestTriggered;
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        if (sequence == GameplaySequence.Chase)
            EnemyTriggerCount++;
    }

    void HandleAnomalyQuestTriggered()
    {
        AnomalyQuestTriggerCount++;
    }

    public void ResetStats()
    {
        EnemyTriggerCount = 0;
        AnomalyQuestTriggerCount = 0;
    }
}
