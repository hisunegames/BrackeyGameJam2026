using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ElectricBoxActivationController : MonoBehaviour
{
    [SerializeField] Transform _cameraTransform;
    [SerializeField] InputActionReference _activateInputAction;
    [SerializeField] float _activateRange = 3f;
    [SerializeField] LayerMask _activatableLayers = ~0;
    [SerializeField] float _holdDuration = 2f;
    [SerializeField] Image _holdFillBar;
    [SerializeField] float _fillDrainSpeed = 2f;
    [SerializeField] Color _fillingColor = Color.white;

    ElectricBoxLever _currentTarget;
    float _holdTimer;
    bool _hasTriggered;
    float _displayedFill;

    void OnEnable()
    {
        _activateInputAction.action.Enable();
    }

    void OnDisable()
    {
        _activateInputAction.action.Disable();
    }

    void Update()
    {
        float targetFill = UpdateHoldState();

        if (_hasTriggered)
        {
            _displayedFill = 0f;
            HideFillBar();
            return;
        }

        _displayedFill = Mathf.MoveTowards(_displayedFill, targetFill, _fillDrainSpeed * Time.deltaTime);
        UpdateFillBar(_displayedFill);
    }

    float UpdateHoldState()
    {
        if (!_activateInputAction.action.IsPressed())
        {
            ResetHold();
            return 0f;
        }

        ElectricBoxLever target = FindTarget();

        if (target != _currentTarget)
        {
            _currentTarget = target;
            _holdTimer = 0f;
            _hasTriggered = false;
        }

        if (_currentTarget == null || _hasTriggered || _currentTarget.IsFixed || !_currentTarget.IsAccessible)
            return 0f;

        _holdTimer += Time.deltaTime;

        if (_holdTimer >= _holdDuration)
        {
            _currentTarget.Fix();
            _hasTriggered = true;
            return 0f;
        }
        // Debug.Log($"Hold Timer: {_holdTimer}, Hold Duration: {_holdDuration}");
        return _holdTimer / _holdDuration;
    }

    void ResetHold()
    {
        _currentTarget = null;
        _holdTimer = 0f;
        _hasTriggered = false;
    }

    void UpdateFillBar(float fillAmount)
    {
        if (_holdFillBar == null)
            return;

        _holdFillBar.enabled = true;
        _holdFillBar.fillAmount = Mathf.Clamp01(fillAmount);
        _holdFillBar.color = _fillingColor;
    }

    void HideFillBar()
    {
        if (_holdFillBar != null)
            _holdFillBar.enabled = false;
    }

    ElectricBoxLever FindTarget()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _activateRange, _activatableLayers, QueryTriggerInteraction.Collide)
            && hit.collider.TryGetComponent(out ElectricBoxLever lever))
        {
            return lever;
        }

        return null;
    }
}
