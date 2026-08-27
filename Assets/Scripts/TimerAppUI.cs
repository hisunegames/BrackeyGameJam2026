using TMPro;
using UnityEngine;

public class TimerAppUI : MonoBehaviour
{
    [SerializeField] Febucci.UI.Core.TAnimCore _textAnimator;

    string _lastText;

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

        string newText = GameManager.Instance.CurrentSequence == GameplaySequence.Chase
            ? "<shake a=1 d=0.1>RUN!!</>"
            : FormatHideText();

        if (newText == _lastText)
            return;

        _lastText = newText;
        _textAnimator.SetText(newText);
    }

    string FormatHideText()
    {
        float remainingTime = PenaltyManager.Instance != null ? PenaltyManager.Instance.RemainingTime : 0f;
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
        return $"<shake a=0.3 d=0.1>HIDE!! {seconds / 60:00}:{seconds % 60:00}</>";
    }
}
