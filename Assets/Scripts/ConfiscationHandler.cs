using UnityEngine;

public class ConfiscationHandler : InteractableObject
{
    [SerializeField] NPCIdentity _npcIdentity;
    [SerializeField] ItemData _item;

    public bool HasBeenConfiscated { get; private set; }

    public override bool CanInteract => base.CanInteract && !HasBeenConfiscated
        && PlayerInventory.Instance != null && !PlayerInventory.Instance.HasItem;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (HasBeenConfiscated || PlayerInventory.Instance == null || PlayerInventory.Instance.HasItem)
            return;

        HasBeenConfiscated = true;

        PlayerInventory.Instance.TryConfiscate(_item);

        bool isAnomaly = _npcIdentity != null && _npcIdentity.IsAnomaly;
        if (isAnomaly && GameManager.Instance != null)
            GameManager.Instance.SetSequence(GameplaySequence.Penalty);
    }
}
