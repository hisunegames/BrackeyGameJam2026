using PixelCrushers.DialogueSystem;
using UnityEngine;

// Bridges our custom PlayerInteractor/IInteractable raycast-interact system to Dialogue System's
// own DialogueSystemTrigger for items authored with Trigger = On Use (pickup/quest-success logic
// lives once in that trigger's luaCode/On Execute, not duplicated here). Nothing else in the scene
// sends these an OnUse message - Dialogue System's own Selector/Usable component, which normally
// would, isn't used anywhere in this project.
public class UsableItemInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string _promptText = "Pick Up";
    [SerializeField] DialogueSystemTrigger _useTrigger;

    public string InteractPrompt => _promptText;
    public bool CanInteract => _useTrigger != null;

    void Awake()
    {
        if (_useTrigger == null)
            _useTrigger = GetComponent<DialogueSystemTrigger>();
    }

    public void Interact(GameObject interactor)
    {
        if (_useTrigger != null)
            _useTrigger.OnUse(interactor != null ? interactor.transform : null);
    }
}
