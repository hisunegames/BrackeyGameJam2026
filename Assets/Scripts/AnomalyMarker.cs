using UnityEngine;

public class AnomalyMarker : MonoBehaviour
{
    [SerializeField] SpriteRenderer _minimapMarkerSprite;
    [SerializeField] Color _normalColor = Color.white;
    [SerializeField] Color _markedColor = Color.yellow;
    [SerializeField] Color _questColor = Color.blue;

    public bool IsMarked { get; private set; }
    public bool IsQuestMarked { get; private set; }

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

    public void SetQuestMarked(bool questMarked)
    {
        IsQuestMarked = questMarked;
        RefreshColor();
        Debug.Log("Marked");
    }

    void RefreshColor()
    {
        if (_minimapMarkerSprite == null)
            return;

        // Manual anomaly suspicion takes visual priority over an automatic quest reminder.
        _minimapMarkerSprite.color = IsMarked ? _markedColor : (IsQuestMarked ? _questColor : _normalColor);
    }
}
