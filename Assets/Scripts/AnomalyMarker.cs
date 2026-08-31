using UnityEngine;

public enum QuestMarkerState
{
    None,
    Available,
    Accepted
}

public class AnomalyMarker : MonoBehaviour
{
    [SerializeField] SpriteRenderer _minimapMarkerSprite;
    [SerializeField] Color _normalColor = Color.white;
    [SerializeField] Color _markedColor = Color.yellow;
    [SerializeField] Color _questAvailableColor = Color.blue;
    [SerializeField] Color _questAcceptedColor = Color.green;

    public bool IsMarked { get; private set; }
    public QuestMarkerState QuestState { get; private set; } = QuestMarkerState.None;

    void Awake()
    {
        RefreshColor();
    }

    public void ToggleMark()
    {
        SetMarked(!IsMarked);
    }

    public void SetMarked(bool marked)
    {
        IsMarked = marked;
        RefreshColor();
    }

    public void SetQuestState(QuestMarkerState state)
    {
        QuestState = state;
        RefreshColor();
    }

    void RefreshColor()
    {
        if (_minimapMarkerSprite == null)
            return;

        // Manual anomaly suspicion takes visual priority over the automatic quest indicator.
        if (IsMarked)
        {
            _minimapMarkerSprite.color = _markedColor;
            return;
        }

        _minimapMarkerSprite.color = QuestState switch
        {
            QuestMarkerState.Available => _questAvailableColor,
            QuestMarkerState.Accepted => _questAcceptedColor,
            _ => _normalColor
        };
    }
}
