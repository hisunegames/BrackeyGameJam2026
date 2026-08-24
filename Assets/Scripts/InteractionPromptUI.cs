using UnityEngine;
using Febucci.UI.Core;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] PlayerInteractor _interactor;
    [SerializeField] TAnimCore _promptText;

    string _lastText;

    void Update()
    {
        IInteractable target = _interactor.CurrentTarget;
        bool hasTarget = target != null;

        string newText = hasTarget ? $"<wiggle a=0.1 f=0.2>Press {_interactor.InteractKeyDisplay} to {target.InteractPrompt}</>" : "";
        if (newText == _lastText)
            return;

        _lastText = newText;
        _promptText.SetText(newText);
    }
}
