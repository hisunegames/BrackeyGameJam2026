using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnomalyMarkingController : MonoBehaviour
{
    [SerializeField] Transform _cameraTransform;
    [SerializeField] InputActionReference _markInputAction;
    [SerializeField] float _markRange = 5f;
    [SerializeField] LayerMask _markableLayers = ~0;
    [SerializeField] float _holdDuration = 1f;
    [SerializeField] Image _holdFillBar;
    [SerializeField] float _fillDrainSpeed = 2f;
    [SerializeField] Color _fillingColor = Color.white;
    [SerializeField] Color _completeColor = Color.green;

    AnomalyMarker _currentTarget;
    float _holdTimer;
    bool _hasTriggered;
    float _displayedFill;

    void OnEnable()
    {
        _markInputAction.action.Enable();
    }

    void OnDisable()
    {
        _markInputAction.action.Disable();
    }

    void Update()
    {
        float targetFill = UpdateHoldState();

        _displayedFill = Mathf.MoveTowards(_displayedFill, targetFill, _fillDrainSpeed * Time.deltaTime);
        UpdateFillBar(_displayedFill, _hasTriggered);
    }

    float UpdateHoldState()
    {
        if (!_markInputAction.action.IsPressed())
        {
            ResetHold();
            return 0f;
        }

        AnomalyMarker target = FindTarget();

        if (target != _currentTarget)
        {
            _currentTarget = target;
            _holdTimer = 0f;
            _hasTriggered = false;
        }

        if (_currentTarget == null || _hasTriggered)
            return 0f;

        _holdTimer += Time.deltaTime;

        if (_holdTimer >= _holdDuration)
        {
            _currentTarget.ToggleMark();
            _hasTriggered = true;
            return 0f;
        }

        return _holdTimer / _holdDuration;
    }

    void ResetHold()
    {
        _currentTarget = null;
        _holdTimer = 0f;
        _hasTriggered = false;
    }

    void UpdateFillBar(float fillAmount, bool isComplete)
    {
        if (_holdFillBar == null)
            return;

        _holdFillBar.fillAmount = Mathf.Clamp01(fillAmount);
        _holdFillBar.color = isComplete ? _completeColor : _fillingColor;
    }

    AnomalyMarker FindTarget()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _markRange, _markableLayers, QueryTriggerInteraction.Collide)
            && hit.collider.TryGetComponent(out AnomalyMarker marker))
        {
            return marker;
        }

        return null;
    }
}
