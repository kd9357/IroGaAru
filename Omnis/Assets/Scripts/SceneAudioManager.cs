// TeamTwo

/*
 * Include Files
 */

 using UnityEngine;

/*
 * Typedefs
 */

public class SceneAudioManager : MonoBehaviour
{
    /*
     * Public Member Variables
     */

    public AudioClip[] AudioClips;

    public float Volume
    {
        get { return _audioSource.volume; }
        set { _audioSource.volume = value; }
    }

    public bool Mute
    {
        set { _audioSource.mute = value; }
    }

    /*
     * Private Member Variables
     */

    private const float DEFAULT_VOLUME = .25f;
    private AudioSource _audioSource;

    /*
     * Public Method Declarations
     */

    void Start()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        _audioSource.volume = DEFAULT_VOLUME;
    }
   
    public void PlayGameOver()
    {
        _audioSource.volume = DEFAULT_VOLUME;
        _audioSource.clip = AudioClips[0];
        _audioSource.Play();
    }
    public void PlayLevelComplete()
    {
        _audioSource.volume = DEFAULT_VOLUME;
        _audioSource.clip = AudioClips[1];
        _audioSource.volume = .5f;
        _audioSource.Play();
    }
    public void PlayLevelSelect()
    {
        _audioSource.volume = DEFAULT_VOLUME;
        _audioSource.clip = AudioClips[2];
        _audioSource.Play();
    }

    public void PlayEnemyDeath()
    {
        _audioSource.volume = DEFAULT_VOLUME;
        _audioSource.clip = AudioClips[3];
        _audioSource.volume = .5f;
        _audioSource.Play();
    }
}