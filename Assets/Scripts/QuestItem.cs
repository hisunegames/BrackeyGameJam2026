using PixelCrushers.DialogueSystem;
using UnityEngine;

public class QuestItem : InteractableObject
{
    [Header("Quest Item")]
    [SerializeField] string _itemId = "Sunglasses1989";

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        string variableName = $"{_itemId}.NumCollected";
        double currentCount = DialogueLua.GetVariable(variableName).asFloat;
        DialogueLua.SetVariable(variableName, currentCount + 1);

        gameObject.SetActive(false);
    }
}
