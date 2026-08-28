using UnityEngine;

public class ElectricBoxLever : InteractableObject
{
    [SerializeField] LEDColorSwitch _ledSwitch;
    [SerializeField] Animator _leverAnimator;
    [SerializeField] string _leverOnTrigger = "SwitchOn";

    public bool IsFixed { get; private set; }
    public bool IsAccessible { get; private set; }

    public override bool CanInteract => base.CanInteract && !IsFixed;

    public void SetAccessible(bool accessible)
    {
        IsAccessible = accessible;
    }

    public void Fix()
    {
        if (IsFixed || !IsAccessible)
            return;

        IsFixed = true;

        if (_ledSwitch != null)
            _ledSwitch.SetSecondColor(true);

        if (_leverAnimator != null)
            _leverAnimator.SetTrigger(_leverOnTrigger);

        if (PenaltyManager.Instance != null)
            PenaltyManager.Instance.NotifyElectricBoxFixed();
    }
}
