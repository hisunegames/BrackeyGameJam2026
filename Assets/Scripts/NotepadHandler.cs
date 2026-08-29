using UnityEngine;

public class NotepadHandler : InteractableObject
{
    [SerializeField] NoteData _noteData;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _pickupSound;

    NoteData[] _notes;

    // Called by whatever spawns this notepad with a day-specific set of notes (e.g. NotepadPodiumSpawner).
    // Overrides the single serialized _noteData for hand-placed notepads.
    public void SetNotes(NoteData[] notes)
    {
        _notes = notes;
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (_notes != null && _notes.Length > 0)
            NotepadUIManager.Instance.OpenNotepad(_notes);
        else
            NotepadUIManager.Instance.OpenNotepad(_noteData);

        if (_audioSource != null && _pickupSound != null)
            _audioSource.PlayOneShot(_pickupSound);
    }
}
