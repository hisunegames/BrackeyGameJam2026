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
    [SerializeField] string[] _questPool = new string[]
    {
        "BillionaireQuest",
        "CookingMamaQuest",
        "BlindGuyQuest",
        "FemaleStudentQuest",
        "GlassesGirlQuest",
        "ProfessorQuest",
        "OtakuQuest",
        "PranQuest",
        "TiktokerQuest"
    };
    [SerializeField] int _mainQuestCount = 4;

    string[] _mainQuestNames = Array.Empty<string>();

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

    // Called by whatever marks the start of a shift (e.g. PlayerSpawnManager, when the player
    // walks out of the spawn area) - the timer/day no longer starts automatically on Gameplay
    // state, since that fired the same moment every day whether or not the player was ready.
    public void BeginShift()
    {
        if (IsShiftActive)
            return;

        StartDay(CurrentDay);
    }

    void StartDay(int day)
    {
        CurrentDay = day;
        RemainingShiftTime = _shiftDuration;
        IsShiftActive = true;
        CanClockOut = false;
        _mainQuestNames = PickRandomQuests();

        OnNewDayStarted?.Invoke(CurrentDay);
    }

    string[] PickRandomQuests()
    {
        if (_questPool == null || _questPool.Length == 0)
            return Array.Empty<string>();

        List<string> pool = new(_questPool);
        int count = Mathf.Min(_mainQuestCount, pool.Count);
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result[i] = pool[index];
            pool.RemoveAt(index);
        }

        return result;
    }

    void Update()
    {
        // GameState only tells us Menu/Gameplay/Pause - it stays Gameplay during Penalty/Chase,
        // so the sequence needs its own check or the timer keeps bleeding away while the player
        // is busy hiding from the monster instead of working.
        if (!IsShiftActive || CanClockOut || GameManager.Instance == null
            || GameManager.Instance.CurrentState != GameState.Gameplay
            || GameManager.Instance.CurrentSequence != GameplaySequence.Normal)
            return;

        RemainingShiftTime -= Time.deltaTime;

        if (RemainingShiftTime <= 0f)
        {
            RemainingShiftTime = 0f;

            // Ran out the clock without finishing the main objectives - that's a failure, not
            // just a normal clock-out. Finishing right as the timer hits zero still counts as
            // a pass, same as before.
            if (!AreMainQuestsComplete())
            {
                FailShift();
                return;
            }
        }

        if (RemainingShiftTime <= 0f || AreMainQuestsComplete())
            SetCanClockOut(true);
    }

    void FailShift()
    {
        if (!IsShiftActive)
            return;

        IsShiftActive = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent(out PlayerController playerController))
            playerController.Die(PlayerController.DeathCause.Fired);
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

    // Shared by GameOverUI and TimerAppUI so both read the same completed/total accounting.
    public void GetMainQuestProgress(out int completed, out int total)
    {
        completed = 0;
        total = 0;

        foreach (string questName in _mainQuestNames)
        {
            if (string.IsNullOrEmpty(questName))
                continue;

            total++;
            if (QuestLog.IsQuestInStateMask(questName, QuestState.Success))
                completed++;
        }
    }

    // Call this from whatever "clock out" door/GameObject the player interacts with.
    // Does NOT start the next day - that waits for BeginShift() (e.g. leaving the spawn area again).
    public void ClockOut()
    {
        if (!IsShiftActive || !CanClockOut)
            return;

        IsShiftActive = false;
        int finishedDay = CurrentDay;
        CurrentDay++;

        OnClockedOut?.Invoke(finishedDay);
    }
}
