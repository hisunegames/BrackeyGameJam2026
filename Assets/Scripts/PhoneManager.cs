using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneManager : MonoBehaviour
{
    [SerializeField] GameObject[] _appPanels;
    [SerializeField] GameObject _notesPanel;
    [SerializeField] GameObject _timerPanel;

    int _currentAppIndex;
    int _notesSlotIndex = -1;

    void Awake()
    {
        if (_notesPanel != null)
            _notesSlotIndex = System.Array.IndexOf(_appPanels, _notesPanel);
    }

    void OnEnable()
    {
        ShowCurrentApp();
        GameManager.OnSequenceChanged += HandleSequenceChanged;
    }

    void OnDisable()
    {
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            SwitchToNextApp();
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        if (_notesSlotIndex < 0 || _timerPanel == null)
            return;

        bool isPenaltyOrChase = sequence != GameplaySequence.Normal;
        _appPanels[_notesSlotIndex] = isPenaltyOrChase ? _timerPanel : _notesPanel;

        if (_notesPanel != null)
            _notesPanel.SetActive(false);
        if (_timerPanel != null)
            _timerPanel.SetActive(false);

        ShowCurrentApp();
    }

    void SwitchToNextApp()
    {
        if (_appPanels.Length == 0)
            return;

        _currentAppIndex = (_currentAppIndex + 1) % _appPanels.Length;
        ShowCurrentApp();
    }

    void ShowCurrentApp()
    {
        for (int i = 0; i < _appPanels.Length; i++)
        {
            if (_appPanels[i] != null)
                _appPanels[i].SetActive(i == _currentAppIndex);
        }
    }
}
