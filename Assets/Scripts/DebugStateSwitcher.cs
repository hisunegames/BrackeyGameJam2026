using UnityEngine;
using UnityEngine.InputSystem;

public class DebugStateSwitcher : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Update()
    {
        if (GameManager.Instance == null || Keyboard.current == null)
            return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            SetState(GameState.Menu);
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
            SetState(GameState.Gameplay);
        else if (Keyboard.current.f3Key.wasPressedThisFrame)
            SetState(GameState.Pause);
        else if (Keyboard.current.f5Key.wasPressedThisFrame)
            SetSequence(GameplaySequence.Normal);
        else if (Keyboard.current.f6Key.wasPressedThisFrame)
            SetSequence(GameplaySequence.Penalty);
        else if (Keyboard.current.f7Key.wasPressedThisFrame)
            SetSequence(GameplaySequence.Chase);
        else if (Keyboard.current.f8Key.wasPressedThisFrame)
            ToggleElectricBoxes();
        else if (Keyboard.current.f9Key.wasPressedThisFrame)
            ToggleNotepadPodiums();
        else if (Keyboard.current.f10Key.wasPressedThisFrame)
            ToggleHidingSpots();
        else if (Keyboard.current.f11Key.wasPressedThisFrame)
            ForceCanClockOut();
        else if (Keyboard.current.f12Key.wasPressedThisFrame)
            FillQuestPoolWithPlaceholders();
    }

    void ToggleElectricBoxes()
    {
        if (ElectricBoxSpawner.Instance == null)
        {
            Debug.Log("[DebugStateSwitcher] No ElectricBoxSpawner in scene");
            return;
        }

        ElectricBoxSpawner.Instance.DebugToggleSpawnAll();
        Debug.Log("[DebugStateSwitcher] Toggled all electric box spawn points");
    }

    void ToggleNotepadPodiums()
    {
        if (NotepadPodiumSpawner.Instance == null)
        {
            Debug.Log("[DebugStateSwitcher] No NotepadPodiumSpawner in scene");
            return;
        }

        NotepadPodiumSpawner.Instance.DebugToggleSpawnAll();
        Debug.Log("[DebugStateSwitcher] Toggled all notepad podium spawn points");
    }

    void ToggleHidingSpots()
    {
        if (HidingSpotManager.Instance == null)
        {
            Debug.Log("[DebugStateSwitcher] No HidingSpotManager in scene");
            return;
        }

        HidingSpotManager.Instance.DebugToggleAllSpots();
        Debug.Log("[DebugStateSwitcher] Toggled all hiding spot doors");
    }

    void ForceCanClockOut()
    {
        if (ShiftManager.Instance == null)
        {
            Debug.Log("[DebugStateSwitcher] No ShiftManager in scene");
            return;
        }

        ShiftManager.Instance.DebugForceCanClockOut();
        Debug.Log("[DebugStateSwitcher] Forced CanClockOut = true");
    }

    void FillQuestPoolWithPlaceholders()
    {
        if (NPCQuestPoolManager.Instance == null)
        {
            Debug.Log("[DebugStateSwitcher] No NPCQuestPoolManager in scene");
            return;
        }

        NPCQuestPoolManager.Instance.DebugFillWithPlaceholders();
        Debug.Log("[DebugStateSwitcher] Filled quest pool with placeholder quest givers");

        // Podiums that already spawned this day cached their notes at spawn time (before this
        // pool fill happened) - push the refreshed pool into them too, or they'd stay empty.
        if (NotepadPodiumSpawner.Instance != null)
        {
            NotepadPodiumSpawner.Instance.DebugRefreshNotes();
            Debug.Log("[DebugStateSwitcher] Refreshed notes on already-spawned podiums");
        }
    }

    void SetState(GameState state)
    {
        GameManager.Instance.SetState(state);
        Debug.Log($"[DebugStateSwitcher] GameState -> {state}");
    }

    void SetSequence(GameplaySequence sequence)
    {
        GameManager.Instance.SetSequence(sequence);
        Debug.Log($"[DebugStateSwitcher] GameplaySequence -> {sequence}");
    }
#endif
}
