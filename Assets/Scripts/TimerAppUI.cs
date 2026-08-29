using TMPro;
using UnityEngine;

public class TimerAppUI : MonoBehaviour
{
    [SerializeField] Febucci.UI.Core.TAnimCore _textAnimator;
    [SerializeField] string _chaseText = "<shake a=1 d=0.1>RUN!!</>";
    [SerializeField] string _findBoxText = "<shake a=0.3 d=0.1>FIND THE ELECTRIC BOX!!</>";
    [SerializeField] string _hideTextFormat = "<shake a=0.3 d=0.1>HIDE!! {0}</>";
    [SerializeField] Vector2 _chaseTextOffset;
    [SerializeField] Vector2 _findBoxTextOffset;
    [SerializeField] Vector2 _hideTextOffset;

    RectTransform _textRect;
    Vector2 _baseAnchoredPosition;
    string _lastText;

    void Awake()
    {
        if (_textAnimator == null)
            return;

        _textRect = _textAnimator.GetComponent<RectTransform>();
        if (_textRect != null)
            _baseAnchoredPosition = _textRect.anchoredPosition;
    }

    void OnEnable()
    {
        PenaltyManager.OnPenaltyTimerChanged += HandleTimerChanged;
        GameManager.OnSequenceChanged += HandleSequenceChanged;
        RefreshDisplay();
    }

    void OnDisable()
    {
        PenaltyManager.OnPenaltyTimerChanged -= HandleTimerChanged;
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
    }

    void HandleTimerChanged(float remainingTime)
    {
        RefreshDisplay();
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (_textAnimator == null || GameManager.Instance == null)
            return;

        string newText;
        Vector2 offset;

        if (GameManager.Instance.CurrentSequence == GameplaySequence.Chase)
        {
            newText = _chaseText;
            offset = _chaseTextOffset;
        }
        else if (PenaltyManager.Instance != null && !PenaltyManager.Instance.IsElectricBoxFixed)
        {
            newText = _findBoxText;
            offset = _findBoxTextOffset;
        }
        else
        {
            float remainingTime = PenaltyManager.Instance != null ? PenaltyManager.Instance.RemainingTime : 0f;
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
            newText = string.Format(_hideTextFormat, $"{seconds / 60:00}:{seconds % 60:00}");
            offset = _hideTextOffset;
        }

        if (_textRect != null)
            _textRect.anchoredPosition = _baseAnchoredPosition + offset;

        if (newText == _lastText)
            return;

        _lastText = newText;
        _textAnimator.SetText(newText);
    }
}
