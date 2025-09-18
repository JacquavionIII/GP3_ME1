using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSourceGeneral;
    [SerializeField] AudioSource sfxSourcePlayer;
    [SerializeField] AudioSource sfxSourceEnemy;

    [Header("Audio Clips")]
    public AudioClip bgmNormal;
    public AudioClip bgmBattle;
    public AudioClip bgmWin;
    public AudioClip enemygrunt;
    public AudioClip enemyAttack;
    public AudioClip enemyFoundPlayer;
    public AudioClip enemyDie;
    public AudioClip playerAttack;
    public AudioClip playerRun;
    public AudioClip playerJump;
    public AudioClip playerLand;
    public AudioClip playerDie;
    public AudioClip zoneCaught;
    public AudioClip zoneStolen;
    public AudioClip deathScreen;
    public AudioClip menuPause;
    public AudioClip menuConfirmButtons;
    public AudioClip menuBackButtons;
    public AudioClip menuExit;

    public static AudioManager instance;
    public bool notInBattle = true; // Set this to false when in battle
    public bool gameIsWon = false;

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
        if (notInBattle == true)
        {
            musicSource.clip = bgmNormal;
            musicSource.Play();
        }
        else if (notInBattle == false)
        {
            musicSource.clip = bgmBattle;
            musicSource.Play();
        }

        if (gameIsWon == true)
        {
            musicSource.clip = bgmWin;
            musicSource.Play();
        }
    }

    public void PlayPlayerSFX(AudioClip clip)
    {
        sfxSourcePlayer.PlayOneShot(clip);
    }
    
    public void PlayEnemySFX(AudioClip clip)
    {
        sfxSourceEnemy.PlayOneShot(clip);
    }
}
