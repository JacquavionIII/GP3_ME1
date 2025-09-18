using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusicNormal;
    public AudioClip backgroundMusicBattle;
    public AudioClip discardCard;
    public AudioClip drawCard;
    public AudioClip placeCardDown;
    public AudioClip pickupPaper;
    public AudioClip menuButtons;

    public static AudioManager instance;
    public bool notInBattle = true; // Set this to false when in battle

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public void Start()
    {
        if (notInBattle== true)
        {
            musicSource.clip = backgroundMusicNormal;
            musicSource.Play();
        }
        else if (notInBattle== false)
        {
            musicSource.clip = backgroundMusicBattle;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
