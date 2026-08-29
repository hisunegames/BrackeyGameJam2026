using DG.Tweening;
using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Transform[] _doors;
    [SerializeField] float _doorOpenHeight = 3f;
    [SerializeField] float _doorMoveDuration = 1f;

    public bool IsOpen { get; private set; }

    Vector3[] _closedLocalPositions;

    void Awake()
    {
        _closedLocalPositions = new Vector3[_doors.Length];
        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] != null)
                _closedLocalPositions[i] = _doors[i].localPosition;
        }
    }

    public void Open()
    {
        IsOpen = true;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(true);

        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] == null)
                continue;

            Vector3 openPosition = _closedLocalPositions[i] + Vector3.up * _doorOpenHeight;
            _doors[i].DOKill();
            _doors[i].DOLocalMove(openPosition, _doorMoveDuration);
        }
    }

    public void Close()
    {
        IsOpen = false;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(false);

        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] == null)
                continue;

            _doors[i].DOKill();
            _doors[i].DOLocalMove(_closedLocalPositions[i], _doorMoveDuration);
        }
    }
}
