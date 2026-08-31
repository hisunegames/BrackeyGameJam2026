using PixelCrushers.DialogueSystem;
using UnityEngine;

// Put on a GameObject with a large trigger Collider covering the player's spawn/break room.
public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] SpawnDoor _spawnDoor;

    [Header("Forced Intro")]
    [SerializeField] DialogueSystemTrigger _introTrigger;
    [SerializeField] GameObject _telephoneAnswerImage;
    [SerializeField] GameObject _playerPhone;

    bool _introPlayed;

    void OnEnable()
    {
        ShiftManager.OnNewDayStarted += HandleNewDayStarted;
        ShiftManager.OnCanClockOutChanged += HandleCanClockOutChanged;
        GameManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        ShiftManager.OnNewDayStarted -= HandleNewDayStarted;
        ShiftManager.OnCanClockOutChanged -= HandleCanClockOutChanged;
        GameManager.OnStateChanged -= HandleStateChanged;

        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded -= HandleIntroConversationEnd;
    }

    void HandleStateChanged(GameState state)
    {
        if (state != GameState.Gameplay || _introPlayed)
            return;

        _introPlayed = true;

        // Player shouldn't be fiddling with their own phone apps while the forced intro call is
        // playing - swapped out for the telephone-answer prompt until the intro conversation ends.
        if (_playerPhone != null)
            _playerPhone.SetActive(false);

        if (_telephoneAnswerImage != null)
            _telephoneAnswerImage.SetActive(true);

        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded += HandleIntroConversationEnd;

        if (_introTrigger != null)
            _introTrigger.OnUse();
    }

    // The intro is the only conversation that can possibly be active this early (forced, before
    // the player can do anything else) - unsubscribes itself right after so it doesn't also fire
    // for every later, unrelated conversation of the day.
    void HandleIntroConversationEnd(Transform actor)
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded -= HandleIntroConversationEnd;

        if (_telephoneAnswerImage != null)
            _telephoneAnswerImage.SetActive(false);

        if (_playerPhone != null)
            _playerPhone.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Can't skip the intro by walking out before it's even fired.
        if (!_introPlayed)
            return;

        if (ShiftManager.Instance != null)
            ShiftManager.Instance.BeginShift();
    }

    void HandleNewDayStarted(int day)
    {
        if (_spawnDoor != null)
            _spawnDoor.Close();
    }

    void HandleCanClockOutChanged()
    {
        if (ShiftManager.Instance != null && ShiftManager.Instance.CanClockOut && _spawnDoor != null)
            _spawnDoor.Open();
    }
}
