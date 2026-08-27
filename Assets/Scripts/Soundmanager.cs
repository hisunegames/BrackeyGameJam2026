using Language.Lua;
using UnityEngine;

public class Soundmanager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip _menuSong;
    [SerializeField] AudioClip _ambienceSong;
    [SerializeField] AudioClip _taskSong;
    [SerializeField] AudioClip _chaseIntro;
    [SerializeField] AudioClip _chaseChase;
    public Soundmanager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetVolumeForSettings(bool up)
    {
        if (up)
        {
            audioSource.volume += 0.1f;
        }
        else
        {
            audioSource.volume -= 0.1f;
        }
    }

    public void SetSongVolume(float volume)
    {
        audioSource.volume = volume;
    }
    public float GetVolume()
    {
        return audioSource.volume;
    }

    public void SongState(bool state)
    {
        if (state == false)
            audioSource.Pause();
        else
            audioSource.Play();
    }    

    public void ChangeMusic(string Song)
    {
        SetSongVolume(0.4f);
        if (Song == "Menu")
            audioSource.clip = _menuSong;
        else if (Song == "Ambience")
            audioSource.clip = _ambienceSong;
        else if (Song == "Task")
            audioSource.clip = _taskSong;
        else if (Song == "Entity")
        {
            audioSource.clip = _chaseIntro;
            SetSongVolume(0.2f);
        }
        else if (Song == "Chase")
        {
            audioSource.clip = _chaseChase;
            SetSongVolume(0.2f);
        }
            
    }

}
