using DG.Tweening;
using TMPro;
using UnityEngine;

public class IntermissionUI : MonoBehaviour
{
    [SerializeField] GameObject _intermissionPanel;
    [SerializeField] TMP_Text _dayCountText;
    [SerializeField] float _countUpDuration = 1.5f;
    [SerializeField] Ease _countUpEase = Ease.InOutQuad;
    [SerializeField] PlayerLook _playerLook;

    Tween _countTween;

    void OnEnable()
    {
        ShiftManager.OnClockedOut += HandleClockedOut;
    }

    void OnDisable()
    {
        ShiftManager.OnClockedOut -= HandleClockedOut;
    }

    void HandleClockedOut(int finishedDay)
    {
        if (_intermissionPanel != null)
            _intermissionPanel.SetActive(true);

        if (_playerLook != null)
            _playerLook.SetLookEnabled(false);

        // Freezes gameplay behind the intermission, same as the main menu's own approach -
        // the count-up tween below runs on unscaled time so it still plays through the freeze.
        Time.timeScale = 0f;

        _countTween?.Kill();

        if (_dayCountText != null)
        {
            int newDay = finishedDay + 1;
            _dayCountText.text = finishedDay.ToString();

            _countTween = DOVirtual.Float(finishedDay, newDay, _countUpDuration,
                value => _dayCountText.text = Mathf.RoundToInt(value).ToString())
                .SetEase(_countUpEase)
                .SetUpdate(true);
        }
    }

    // Wire this to the intermission panel's "Continue" button OnClick.
    public void Continue()
    {
        _countTween?.Kill();

        if (_intermissionPanel != null)
            _intermissionPanel.SetActive(false);

        if (_playerLook != null)
            _playerLook.SetLookEnabled(true);

        Time.timeScale = 1f;
    }
}
