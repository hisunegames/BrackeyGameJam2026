using UnityEngine;
using Febucci.UI.Core;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] PlayerInteractor _interactor;
    [SerializeField] TAnimCore _promptText;

    string _lastText;
    string newText;

    void Update()
    {
        IInteractable target = _interactor.CurrentTarget;
        bool hasTarget = target != null;

        if (hasTarget && target.InteractPrompt == "Reset Lights")
            newText = hasTarget ? $"<wiggle a=0.1 f=0.2>Hold E to {target.InteractPrompt}</>" : "";
        else
            newText = hasTarget ? $"<wiggle a=0.1 f=0.2>Press E to {target.InteractPrompt}</>" : "";
        if (newText == _lastText)
            return;

        _lastText = newText;
        _promptText.SetText(newText);
    }
}
