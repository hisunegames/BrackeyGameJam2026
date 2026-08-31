using UnityEngine;

public class ShiftClockOutPodium : InteractableObject
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _ringSound;

    public override bool CanInteract => base.CanInteract
        && ShiftManager.Instance != null && ShiftManager.Instance.CanClockOut;

    void OnEnable()
    {
        ShiftManager.OnCanClockOutChanged += HandleCanClockOutChanged;
    }

    void OnDisable()
    {
        ShiftManager.OnCanClockOutChanged -= HandleCanClockOutChanged;
    }

    void HandleCanClockOutChanged()
    {
        if (ShiftManager.Instance != null && ShiftManager.Instance.CanClockOut)
            PlayRing();
        else
            StopRing();
    }

    void PlayRing()
    {
        if (_audioSource == null || _ringSound == null)
            return;

        _audioSource.clip = _ringSound;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    void StopRing()
    {
        if (_audioSource != null)
            _audioSource.Stop();
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        Debug.Log("Clocking out from phone");

        // Answered the "call" - stop ringing immediately regardless of what ClockOut() itself does.
        StopRing();

        if (ShiftManager.Instance != null)
            ShiftManager.Instance.ClockOut();
    }
}
