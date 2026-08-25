using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] string _promptText = "Interact";
    [SerializeField] bool _interactOnce = false;
    [SerializeField] UnityEvent _onInteract;

    bool _hasBeenInteractedWith;

    public string InteractPrompt => _promptText;

    public virtual void Interact(GameObject interactor)
    {
        if (_interactOnce && _hasBeenInteractedWith)
            return;

        _hasBeenInteractedWith = true;
        _onInteract.Invoke();
    }
}
