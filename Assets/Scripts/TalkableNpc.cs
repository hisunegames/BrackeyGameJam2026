using PixelCrushers.DialogueSystem;
using UnityEngine;

// Bridges our custom PlayerInteractor/IInteractable raycast-interact system to Dialogue System's
// own DialogueSystemTrigger for NPCs (Trigger = On Use) - same reasoning as UsableItemInteractable,
// but as an InteractableObject subclass so the existing "Talk" prompt/_interactOnce config on the
// NPC prefab keeps working. Crucially, this calls OnUse(actor) with the real interactor's transform
// instead of the parameterless OnUse() - a null actor makes Dialogue System fall back to resolving
// the "actor" from the conversation's own database Actor entry, which isn't guaranteed to resolve
// to the live player instance, so OnConversationStart/End (and everything hanging off it, like
// PlayerController's movement lock) can silently never reach the player.
public class TalkableNpc : InteractableObject
{
    [SerializeField] DialogueSystemTrigger _dialogueTrigger;

    void Awake()
    {
        if (_dialogueTrigger == null)
            _dialogueTrigger = GetComponent<DialogueSystemTrigger>();
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (_dialogueTrigger != null)
            _dialogueTrigger.OnUse(interactor != null ? interactor.transform : null);
    }
}
