using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ShiftManager : MonoBehaviour
{
    public static ShiftManager Instance { get; private set; }

    [Header("Shift Timer")]
    [SerializeField] float _shiftDuration = 300f;

    [Header("Main Quests")]
    [SerializeField] string[] _mainQuestNames = new string[4];

    public IReadOnlyList<string> MainQuestNames => _mainQuestNames;

    public int CurrentDay { get; private set; } = 1;
    public float RemainingShiftTime { get; private set; }
    public bool IsShiftActive { get; private set; }
    public bool CanClockOut { get; private set; }

    public static event Action<int> OnNewDayStarted;
    public static event Action OnCanClockOutChanged;
    public static event Action<int> OnClockedOut;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (state == GameState.Gameplay && !IsShiftActive)
            StartDay(CurrentDay);
    }

    void StartDay(int day)
    {
        CurrentDay = day;
        RemainingShiftTime = _shiftDuration;
        IsShiftActive = true;
        CanClockOut = false;

        OnNewDayStarted?.Invoke(CurrentDay);
    }

    void Update()
    {
        if (!IsShiftActive || CanClockOut || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Gameplay)
            return;

        RemainingShiftTime -= Time.deltaTime;
        if (RemainingShiftTime <= 0f || AreMainQuestsComplete())
            SetCanClockOut(true);
    }

    bool AreMainQuestsComplete()
    {
        bool hasAnyQuest = false;

        foreach (string questName in _mainQuestNames)
        {
            if (string.IsNullOrEmpty(questName))
                continue;

            hasAnyQuest = true;

            if (!QuestLog.IsQuestInStateMask(questName, QuestState.Success))
                return false;
        }

        // With zero quest names configured, this must NOT vacuously report "complete" -
        // that would flip CanClockOut true on day 1's first frame and freeze the timer forever.
        return hasAnyQuest;
    }

    void SetCanClockOut(bool canClockOut)
    {
        if (CanClockOut == canClockOut)
            return;

        CanClockOut = canClockOut;
        OnCanClockOutChanged?.Invoke();
    }

    // Debug only - skips waiting on the timer/quests so the clock-out door/day-advance flow
    // can be tested on its own.
    public void DebugForceCanClockOut()
    {
        SetCanClockOut(true);
    }

    // Call this from whatever "clock out" door/GameObject the player interacts with.
    public void ClockOut()
    {
        if (!IsShiftActive || !CanClockOut)
            return;

        IsShiftActive = false;
        OnClockedOut?.Invoke(CurrentDay);
        StartDay(CurrentDay + 1);
    }
}
