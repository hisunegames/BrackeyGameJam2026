using Unity.Cinemachine;
using UnityEngine;

public class ElectricBoxCompass : MonoBehaviour
{
    [SerializeField] RectTransform _arrow;
    [SerializeField] Transform _player;
    [SerializeField] CinemachinePanTilt _cineCamera;

    GameObject _target;
    IInteractable _targetInteractable;

    void Update()
    {
        if (_target == null)
        {
            _target = GameObject.FindGameObjectWithTag("ElectricBox");
            _targetInteractable = _target != null ? _target.GetComponentInChildren<IInteractable>() : null;
        }

        bool isPenaltyActive = GameManager.Instance != null && GameManager.Instance.CurrentSequence == GameplaySequence.Penalty;
        bool shouldShow = isPenaltyActive && _target != null && (_targetInteractable == null || _targetInteractable.CanInteract);
        _arrow.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        Vector3 toTarget = _target.transform.position - _player.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        float relativeAngle = targetAngle - _cineCamera.PanAxis.Value;

        _arrow.localRotation = Quaternion.Euler(0f, 0f, -relativeAngle);
    }
}
