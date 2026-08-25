using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Gameplay,
    Pause
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Menu;

    public static event Action<GameState> OnStateChanged;

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
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Time.timeScale = newState == GameState.Pause ? 0f : 1f;
        OnStateChanged?.Invoke(newState);
    }

    public void StartGame() => SetState(GameState.Gameplay);

    public void PauseGame()
    {
        if (CurrentState == GameState.Gameplay)
            SetState(GameState.Pause);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
            SetState(GameState.Gameplay);
    }

    public void ReturnToMenu() => SetState(GameState.Menu);
}
