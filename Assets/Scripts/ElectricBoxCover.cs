using UnityEngine;

public class ElectricBoxCover : InteractableObject
{
    [SerializeField] Animator _coverAnimator;
    [SerializeField] string _openTrigger = "Open";
    [SerializeField] ElectricBoxLever _lever;

    public bool IsOpen { get; private set; }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (IsOpen)
            return;

        IsOpen = true;

        if (_coverAnimator != null)
            _coverAnimator.SetTrigger(_openTrigger);

        if (_lever != null)
            _lever.SetAccessible(true);
    }
}
