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

    AnomalyMarker _currentTarget;
    float _holdTimer;
    bool _hasTriggered;

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
        if (!_markInputAction.action.IsPressed())
        {
            ResetHold();
            return;
        }

        AnomalyMarker target = FindTarget();

        if (target != _currentTarget)
        {
            _currentTarget = target;
            _holdTimer = 0f;
            _hasTriggered = false;
        }

        if (_currentTarget == null || _hasTriggered)
            return;

        _holdTimer += Time.deltaTime;
        UpdateFillBar(_holdTimer / _holdDuration);

        if (_holdTimer >= _holdDuration)
        {
            _currentTarget.ToggleMark();
            _hasTriggered = true;
        }
    }

    void ResetHold()
    {
        _currentTarget = null;
        _holdTimer = 0f;
        _hasTriggered = false;
        UpdateFillBar(0f);
    }

    void UpdateFillBar(float fillAmount)
    {
        if (_holdFillBar != null)
            _holdFillBar.fillAmount = Mathf.Clamp01(fillAmount);
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
