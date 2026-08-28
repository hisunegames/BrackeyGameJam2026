using UnityEngine;

public class SFX : MonoBehaviour
{

    [SerializeField] AudioSource _audioSource;

    [SerializeField] AudioClip _taskRecieved;
    [SerializeField] AudioClip _taskComplete;
    [SerializeField] AudioClip _breakerSwitch;

    public void PlayMusic(string name)
    {
        if (name == "taskGet")
            _audioSource.clip = _taskRecieved;
        else if (name == "taskDone")
            _audioSource.clip = _taskComplete;
        else if (name == "breaker")
            _audioSource.clip = _breakerSwitch;
        _audioSource.Play();
    }

}
