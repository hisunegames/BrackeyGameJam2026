using PixelCrushers.DialogueSystem;
using UnityEngine;

public class QuestItem : InteractableObject
{
    [Header("Quest")]
    [SerializeField] string _questName;
    [SerializeField] int _questEntryNumber = 1;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (!string.IsNullOrEmpty(_questName))
            QuestLog.SetQuestEntryState(_questName, _questEntryNumber, QuestState.Success);

        gameObject.SetActive(false);
    }
}
