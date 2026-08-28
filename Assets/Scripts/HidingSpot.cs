using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Animator _doorAnimator;
    [SerializeField] string _openTrigger = "Open";
    [SerializeField] string _closeTrigger = "Close";

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(true);

        if (_doorAnimator != null)
            _doorAnimator.SetTrigger(_openTrigger);
    }

    public void Close()
    {
        IsOpen = false;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(false);

        if (_doorAnimator != null)
            _doorAnimator.SetTrigger(_closeTrigger);
    }
}
