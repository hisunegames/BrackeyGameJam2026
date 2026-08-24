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

    private void OnEnable()
    {
        _lookInputAction.action.Enable();
        SetCursorLocked(_lockCursor);
    }

    private void OnDisable()
    {
        _lookInputAction.action.Disable();
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
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(false);
        }
        else if (_lockCursor && !_cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLocked(true);
        }
    }

    void SetCursorLocked(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
