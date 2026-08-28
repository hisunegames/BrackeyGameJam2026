using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Gameplay,
    Pause
}

public enum GameplaySequence
{
    Normal,
    Penalty,
    Chase
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] Soundmanager soundManager;
    [SerializeField] SFX sfxManager;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public GameplaySequence CurrentSequence { get; private set; } = GameplaySequence.Normal;

    public static event Action<GameState> OnStateChanged;
    public static event Action<GameplaySequence> OnSequenceChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        SetState(GameState.Menu);
        soundManager.ChangeMusic("Menu");
        soundManager.SongState(true);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Time.timeScale = newState == GameState.Pause ? 0f : 1f;
        OnStateChanged?.Invoke(newState);
    }

    public void SetSequence(GameplaySequence newSequence)
    {
        if (CurrentSequence == newSequence)
            return;

        CurrentSequence = newSequence;

        if (newSequence == GameplaySequence.Chase)
        {
            soundManager.ChangeMusic("Chase");
            soundManager.SongState(true);
        }
        else if (newSequence == GameplaySequence.Normal)
        {
            soundManager.ChangeMusic("Ambience");
            soundManager.SongState(true);
        }
        else if (newSequence == GameplaySequence.Penalty)
        {
            sfxManager.PlayMusic("breaker");
            soundManager.ChangeMusic("Entity");
            soundManager.SongState(true);
        }


        OnSequenceChanged?.Invoke(newSequence);
    }

    public void StartGame()
    {
        SetState(GameState.Gameplay);
        SetSequence(GameplaySequence.Normal);
        soundManager.ChangeMusic("Ambience");
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Gameplay)
        {
            SetState(GameState.Pause);
            soundManager.SongState(false);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
        {
            SetState(GameState.Gameplay);
            soundManager.SongState(false);
        }
    }

    public void ReturnToMenu()
    {
        SetState(GameState.Menu);
        soundManager.ChangeMusic("Menu");
    }
}
