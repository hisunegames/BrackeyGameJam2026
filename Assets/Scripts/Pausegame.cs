using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pausegame : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject[] soundbar;
    [SerializeField] CinemachineBrain cinemachine;
    [SerializeField] Soundmanager soundmanager;
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Togglepause(!pausePanel.activeSelf);
        }
    }

    void Togglepause(bool isPaused)
    {
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused? 0f:1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
        cinemachine.enabled = !isPaused;
    }

    public void SetVolume(bool isup)
    {
        soundmanager.Instance.SetVolumeForSettings(isup);
        float currentvolume = soundmanager.Instance.GetVolume();

        foreach (GameObject x in soundbar)
        {
            x.SetActive(false);
        }

        int activeBarCount = Mathf.RoundToInt(currentvolume * soundbar.Length);

        for (int i = 0 ; i < activeBarCount ; i++)
        {
            soundbar[i].SetActive(true);
        }
    }
}
