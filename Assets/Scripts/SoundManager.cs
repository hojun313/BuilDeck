using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;

    public AudioClip cardSwapSound;
    public AudioClip buttonClickSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
