using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneManager : MonoBehaviour
{
    [SerializeField] GameObject[] _appPanels;

    int _currentAppIndex;

    void OnEnable()
    {
        ShowCurrentApp();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            SwitchToNextApp();
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
