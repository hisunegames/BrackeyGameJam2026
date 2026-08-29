using UnityEngine;

public class ShiftClockOutPodium : InteractableObject
{
    public override bool CanInteract => base.CanInteract
        && ShiftManager.Instance != null && ShiftManager.Instance.CanClockOut;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (ShiftManager.Instance != null)
            ShiftManager.Instance.ClockOut();
    }
}
