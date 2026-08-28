using UnityEngine;

public class AnomalyMarker : MonoBehaviour
{
    [SerializeField] SpriteRenderer _minimapMarkerSprite;
    [SerializeField] Color _normalColor = Color.white;
    [SerializeField] Color _markedColor = Color.yellow;

    public bool IsMarked { get; private set; }

    void Awake()
    {
        if (_minimapMarkerSprite != null)
            _minimapMarkerSprite.color = _normalColor;
    }

    public void ToggleMark()
    {
        SetMarked(!IsMarked);
    }

    public void SetMarked(bool marked)
    {
        IsMarked = marked;

        if (_minimapMarkerSprite != null)
            _minimapMarkerSprite.color = marked ? _markedColor : _normalColor;
    }
}
