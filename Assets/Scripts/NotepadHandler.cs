using UnityEngine;

public class NotepadHandler : InteractableObject
{
    [SerializeField] NoteData _noteData;
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        NotepadUIManager.Instance.OpenNotepad(_noteData);
    }
}
