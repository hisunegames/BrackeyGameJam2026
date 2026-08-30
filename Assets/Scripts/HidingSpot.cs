using DG.Tweening;
using UnityEngine;

public enum HidingSpotState
{
    Closed,
    SemiOpen,
    Open
}

// Doors must be pivoted at their top-center for this to look right - scaling Z around a
// top pivot keeps the top fixed while the bottom edge grows down (closed) or shrinks up (open).
public class HidingSpot : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Transform[] _doors;
    [SerializeField] float _doorMoveDuration = 1f;

    public HidingSpotState State { get; private set; } = HidingSpotState.Closed;
    public bool IsOpen => State == HidingSpotState.Open;

    float[] _closedScalesZ;

    void Awake()
    {
        _closedScalesZ = new float[_doors.Length];
        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] != null)
                _closedScalesZ[i] = _doors[i].localScale.z;
        }
    }

    public void Open()
    {
        State = HidingSpotState.Open;
        SetLed(true);
        TweenDoorsTo(i => 0.0001f);
    }

    // Doors only crack open enough for the player to crouch/crawl through - used for the
    // limited hiding spots during Penalty, as opposed to fully Open (freely walkable).
    // openFraction is owned by HidingSpotManager so it's tuned globally, not per-door.
    public void SemiOpen(float openFraction)
    {
        State = HidingSpotState.SemiOpen;
        SetLed(true);
        TweenDoorsTo(i => _closedScalesZ[i] * openFraction);
    }

    public void Close()
    {
        State = HidingSpotState.Closed;
        SetLed(false);
        TweenDoorsTo(i => _closedScalesZ[i]);
    }

    void SetLed(bool on)
    {
        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(on);
    }

    void TweenDoorsTo(System.Func<int, float> targetScaleZFor)
    {
        for (int i = 0; i < _doors.Length; i++)
        {
            if (_doors[i] == null)
                continue;

            _doors[i].DOKill();
            _doors[i].DOScaleZ(targetScaleZFor(i), _doorMoveDuration);
        }
    }
}
