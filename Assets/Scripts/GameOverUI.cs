using System.Collections;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject _blackScreen;
    [SerializeField] float _blackScreenDelay = 2f;
    [SerializeField] float _blackScreenHoldDuration = 1f;

    [SerializeField] GameObject _gameOverPanel;
    [SerializeField] TMP_Text _gameOverTitleText;
    [SerializeField] string _killedTitle = "GAME OVER";
    [SerializeField] string _firedTitle = "YOU GOT FIRED";
    [SerializeField] TMP_Text _daysSurvivedText;
    [SerializeField] TMP_Text _normalQuestText;
    [SerializeField] TMP_Text _anomalyQuestText;
    [SerializeField] TMP_Text _keywordsFoundText;
    [SerializeField] TMP_Text _enemyTriggerText;

    PlayerController.DeathCause _deathCause;

    void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDied;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDied;
    }

    void HandlePlayerDied(PlayerController.DeathCause cause)
    {
        _deathCause = cause;
        StartCoroutine(ShowBlackThenGameOver());
    }

    IEnumerator ShowBlackThenGameOver()
    {
        yield return new WaitForSeconds(_blackScreenDelay);

        if (_blackScreen != null)
            _blackScreen.SetActive(true);

        yield return new WaitForSeconds(_blackScreenHoldDuration);

        PopulateStats();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);

        if (_blackScreen != null)
            _blackScreen.SetActive(false);
    }

    void PopulateStats()
    {
        if (_gameOverTitleText != null)
            _gameOverTitleText.text = _deathCause == PlayerController.DeathCause.Fired ? _firedTitle : _killedTitle;

        if (_daysSurvivedText != null)
            _daysSurvivedText.text = (ShiftManager.Instance != null ? ShiftManager.Instance.CurrentDay : 1).ToString();

        if (_normalQuestText != null)
            _normalQuestText.text = FormatNormalQuestProgress();

        if (_anomalyQuestText != null)
            _anomalyQuestText.text = (GameStatsTracker.Instance != null ? GameStatsTracker.Instance.AnomalyQuestTriggerCount : 0).ToString();

        if (_keywordsFoundText != null)
            _keywordsFoundText.text = (NoteAppUIManager.Instance != null ? NoteAppUIManager.Instance.TotalKeywordsFound : 0).ToString();

        if (_enemyTriggerText != null)
            _enemyTriggerText.text = (GameStatsTracker.Instance != null ? GameStatsTracker.Instance.EnemyTriggerCount : 0).ToString();
    }

    string FormatNormalQuestProgress()
    {
        if (ShiftManager.Instance == null)
            return "0/0";

        ShiftManager.Instance.GetMainQuestProgress(out int completed, out int total);
        return $"{completed}/{total}";
    }

    // Wire this to the "Return to Main Menu" button's OnClick. Reloads the whole scene rather
    // than manually resetting each manager - the game is one scene with no separate menu scene,
    // so this is the only way to guarantee NPCs, the player, ShiftManager's day count, hiding
    // spots etc. all actually start fresh instead of carrying over stale state from the run that
    // just ended. DialogueManager persists across scene loads (dontDestroyOnLoad, on by default),
    // so quest states/Lua variables need clearing explicitly - a scene reload alone wouldn't do it.
    public void ReturnToMenu()
    {
        DialogueManager.ResetDatabase();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
