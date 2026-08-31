using DG.Tweening;
using UnityEngine;

public enum HidingSpotState
{
    Closed,
    SemiOpen,
    Open
}

public enum DoorScaleAxis
{
    Y,
    Z
}

public class HidingSpot : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Transform[] _doors;
    [SerializeField] float _doorMoveDuration = 1f;
    [SerializeField] DoorScaleAxis _scaleAxis = DoorScaleAxis.Y;

    public HidingSpotState State { get; private set; } = HidingSpotState.Closed;
    public bool IsOpen => State == HidingSpotState.Open;

    float[] _closedScales;

    void Awake()
    {
        RecomputeClosedScales();
    }

    // Called by HidingSpotManager - recomputes immediately from the doors' current transforms
    // rather than relying on Awake order, since this can be called before this HidingSpot's own
    // Awake has run (Unity doesn't guarantee Awake order across different objects).
    public void SetScaleAxis(DoorScaleAxis axis)
    {
        _scaleAxis = axis;
        RecomputeClosedScales();
    }

    void RecomputeClosedScales()
    {
        _closedScales = new float[_doors.Length];
        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] != null)
                _closedScales[i] = GetAxisValue(_doors[i].localScale);
        }
    }

    float GetAxisValue(Vector3 scale) => _scaleAxis == DoorScaleAxis.Z ? scale.z : scale.y;

    // Fully open just hides the doors and lets the player walk through freely. This is used for the "normal" hiding spots that are always available.
    public void Open()
    {
        State = HidingSpotState.Open;
        SetLed(true);
        TweenDoorsTo(i => 0.001f);
    }

    // Doors only crack open enough for the player to crouch/crawl through - used for the
    // limited hiding spots during Penalty, as opposed to fully Open (freely walkable).
    public void SemiOpen(float openFraction)
    {
        State = HidingSpotState.SemiOpen;
        SetLed(true);
        TweenDoorsTo(i => _closedScales[i] * openFraction);
    }

    public void Close()
    {
        State = HidingSpotState.Closed;
        SetLed(false);
        TweenDoorsTo(i => _closedScales[i]);
    }

    void SetLed(bool on)
    {
        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(on);
    }

    void TweenDoorsTo(System.Func<int, float> targetScaleFor)
    {
        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] == null)
                continue;

            _doors[i].DOKill();
            if (_scaleAxis == DoorScaleAxis.Z)
                _doors[i].DOScaleZ(targetScaleFor(i), _doorMoveDuration);
            else
                _doors[i].DOScaleY(targetScaleFor(i), _doorMoveDuration);
        }
    }
}
