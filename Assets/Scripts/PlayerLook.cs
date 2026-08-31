using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] CinemachinePanTilt _cineCamera;
    [SerializeField] InputActionReference _lookInputAction;
    [SerializeField] float _mouseSensitivity = 0.15f;
    [SerializeField] float _gamepadSensitivity = 120f;
    [SerializeField] bool _invertY = false;
    [SerializeField] bool _lockCursor = true;

    bool _cursorLocked;
    bool _isDead;

    private void OnEnable()
    {
        _lookInputAction.action.Enable();
        // Don't lock/enable look immediately - the scene is already running (frozen under the
        // main menu) at this point, which is exactly the window where the camera could move
        // before real gameplay even starts. Wait for GameManager to actually reach Gameplay.
        SetCursorLocked(false);
        GameManager.OnStateChanged += HandleStateChanged;
        PlayerController.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        _lookInputAction.action.Disable();
        SetCursorLocked(false);
        GameManager.OnStateChanged -= HandleStateChanged;
        PlayerController.OnPlayerDied -= HandlePlayerDied;
    }

    void HandleStateChanged(GameState state)
    {
        if (state == GameState.Gameplay)
            SetCursorLocked(_lockCursor);
    }

    // GameManager.CurrentState never actually leaves Gameplay on death, so without this,
    // HandleCursorToggle would just re-lock the cursor the moment the player clicks the Game
    // Over panel's Return to Menu button (a left-click, which is exactly what it also treats as
    // "lock the cursor again").
    void HandlePlayerDied(PlayerController.DeathCause cause)
    {
        _isDead = true;
        SetCursorLocked(false);
    }

    void Update()
    {
        HandleCursorToggle();

        if (!_cursorLocked)
            return;

        Vector2 lookDelta = _lookInputAction.action.ReadValue<Vector2>();
        bool isGamepad = _lookInputAction.action.activeControl?.device is Gamepad;
        lookDelta *= isGamepad ? _gamepadSensitivity * Time.deltaTime : _mouseSensitivity;

        float pan = _cineCamera.PanAxis.Value + lookDelta.x;
        if (pan > 180f) pan -= 360f;
        else if (pan < -180f) pan += 360f;
        _cineCamera.PanAxis.Value = pan;

        float tiltDelta = _invertY ? lookDelta.y : -lookDelta.y;
        _cineCamera.TiltAxis.Value = Mathf.Clamp(_cineCamera.TiltAxis.Value + tiltDelta, _cineCamera.TiltAxis.Range.x, _cineCamera.TiltAxis.Range.y);
    }

    void HandleCursorToggle()
    {
        if (_isDead)
            return;

        bool canLook = GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Gameplay;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(false);
        }
        else if (canLook && _lockCursor && !_cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLocked(true);
        }
    }

    // Called by Pausegame (and anything else that needs to take over the cursor) so this
    // script's own cursor-locked state stays in sync instead of fighting over Cursor directly.
    public void SetLookEnabled(bool enabled)
    {
        SetCursorLocked(enabled && _lockCursor);
    }

    void SetCursorLocked(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
