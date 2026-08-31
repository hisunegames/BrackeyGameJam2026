using Language.Lua;
using UnityEngine;

public class Soundmanager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource _staticAudioSource;
    [SerializeField] AudioClip _ambienceSong;
    [SerializeField] AudioClip _taskSong;
    [SerializeField] AudioClip _chaseIntro;
    [SerializeField] AudioClip _chaseChase;
    [SerializeField] AudioClip _staticNoise;
    [SerializeField] float _staticVolume = 0.3f;
    [SerializeField] float _masterVolume = 1f;
    public Soundmanager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AudioListener.volume = _masterVolume;
    }

    // AudioListener.volume is a global multiplier Unity applies on top of every AudioSource's
    // own volume - driving it here (instead of this one music AudioSource) means footsteps, SFX,
    // monster/chase sounds etc. all respond to the pause menu's volume control too, without each
    // of those scripts needing their own settings-volume plumbing.
    public void SetVolumeForSettings(bool up)
    {
        _masterVolume = Mathf.Clamp01(_masterVolume + (up ? 0.1f : -0.1f));
        AudioListener.volume = _masterVolume;
    }

    public void SetSongVolume(float volume)
    {
        audioSource.volume = volume;
    }
    public float GetVolume()
    {
        return _masterVolume;
    }

    public void SongState(bool state)
    {
        if (state == false)
        {
            audioSource.Pause();
            if (_staticAudioSource != null)
                _staticAudioSource.Pause();
        }
        else
        {
            audioSource.Play();
            if (_staticAudioSource != null && _staticAudioSource.clip != null)
                _staticAudioSource.Play();
        }
    }

    public void ChangeMusic(string Song)
    {
        SetSongVolume(0.4f);

        // Menu has no actual song - just the ambience track plus a looping static layer, no music.
        if (Song == "Menu")
        {
            audioSource.clip = _ambienceSong;
            SetStatic(true);
        }
        else if (Song == "Ambience")
        {
            audioSource.clip = _ambienceSong;
            SetStatic(false);
        }
        else if (Song == "Task")
        {
            audioSource.clip = _taskSong;
            SetStatic(false);
        }
        else if (Song == "Entity")
        {
            audioSource.clip = _chaseIntro;
            SetSongVolume(0.2f);
            SetStatic(false);
        }
        else if (Song == "Chase")
        {
            audioSource.clip = _chaseChase;
            SetSongVolume(0.2f);
            SetStatic(false);
        }
    }

    void SetStatic(bool play)
    {
        if (_staticAudioSource == null)
            return;

        if (!play)
        {
            _staticAudioSource.Stop();
            return;
        }

        _staticAudioSource.clip = _staticNoise;
        _staticAudioSource.volume = _staticVolume;
        _staticAudioSource.loop = true;
        _staticAudioSource.Play();
    }
}
