using UnityEngine;

public class NotepadHandler : InteractableObject
{
    [SerializeField] NoteData _noteData;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _pickupSound;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        NotepadUIManager.Instance.OpenNotepad(_noteData);

        if (NoteAppUIManager.Instance != null)
            NoteAppUIManager.Instance.RegisterNote(_noteData);

        if (_audioSource != null && _pickupSound != null)
            _audioSource.PlayOneShot(_pickupSound);
    }
}
