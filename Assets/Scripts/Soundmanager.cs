using UnityEngine;

public class Soundmanager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
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

    public float GetVolume()
    {
        return audioSource.volume;
    }
}
