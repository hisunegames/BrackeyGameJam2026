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
