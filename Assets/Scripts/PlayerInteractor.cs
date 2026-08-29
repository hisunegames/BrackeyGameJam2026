using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] Transform _cameraTransform;
    [SerializeField] InputActionReference _interactInputAction;
    [SerializeField] float _interactRange = 3f;
    [SerializeField] LayerMask _interactableLayers = ~0;

    public IInteractable CurrentTarget { get; private set; }
    public string InteractKeyDisplay => _interactInputAction.action.GetBindingDisplayString(group: "Keyboard&Mouse");

    private void OnEnable()
    {
        _interactInputAction.action.Enable();
    }

    private void OnDisable()
    {
        _interactInputAction.action.Disable();
    }

    void Update()
    {
        CurrentTarget = FindTarget();

        if (!_interactInputAction.action.WasPerformedThisFrame())
            return;

        if (NotepadUIManager.Instance != null && NotepadUIManager.Instance.IsOpen)
        {
            NotepadUIManager.Instance.CloseNotepad();
            return;
        }

        if (CurrentTarget != null)
            CurrentTarget.Interact(gameObject);
    }

    IInteractable FindTarget()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactRange, _interactableLayers, QueryTriggerInteraction.Collide)
            && hit.collider.TryGetComponent(out IInteractable interactable)
            && interactable.CanInteract)
        {
            return interactable;
        }
        return null;
    }
}
