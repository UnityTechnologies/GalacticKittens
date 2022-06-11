using System.Collections;
using UnityEngine;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public enum MusicName
    {
        intro,
        gameplay
    };

    [Header("Music")]
    [SerializeField]
    private AudioSource m_introSource;

    [SerializeField]
    private AudioSource m_gameplaySource;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_maxMusicVolume;

    [Header("SFX")]
    [SerializeField]
    private AudioSource m_sfxSource;

    private readonly float k_volumeSteps = 0.01f;

    private void Start()
    {
        // We know that the menu scene if the first to use this script
        PlayMusic(MusicName.intro);
    }
   
    public void PlaySoundEffect(AudioClip clip, float volume = 1f)
    {
        m_sfxSource.PlayOneShot(clip, volume);
    }

    // Play the music of the game without any effect
    public void PlayMusic(MusicName musicToPlay)
    {
        if (musicToPlay == MusicName.intro)
        {
            m_introSource.enabled = true;
            m_introSource.volume = m_maxMusicVolume;
            m_introSource.Play();

            m_gameplaySource.Stop();
            m_gameplaySource.enabled = false;
        }
        else
        {
            m_gameplaySource.enabled = true;
            m_gameplaySource.volume = m_maxMusicVolume;
            m_gameplaySource.Play();

            m_introSource.Stop();
            m_introSource.enabled = false;
        }
    }

    public void SwitchToGameplayMusic()
    {        
        m_gameplaySource.volume = 0f;
        m_gameplaySource.enabled = true;
        m_gameplaySource.Play();

        StartCoroutine(SwitchMusicToPlay(MusicName.gameplay));
    }

    private IEnumerator SwitchMusicToPlay(MusicName musicToPlay)
    {
        yield return FadeInMusicToPlayFadeOutCurrentMusic(musicToPlay);

        StopAudioOfCurrentMusic(musicToPlay);
    }

    private IEnumerator FadeInMusicToPlayFadeOutCurrentMusic(MusicName musicToPlay)
    {
        float volume = 0f;

        // Repeat until the volume go up to the max
        while (volume <= m_maxMusicVolume)
        {
            if (musicToPlay == MusicName.intro)
            {
                m_introSource.volume += k_volumeSteps;
                m_gameplaySource.volume -= k_volumeSteps;
            }
            else
            {
                m_introSource.volume -= k_volumeSteps;
                m_gameplaySource.volume += k_volumeSteps;
            }
            volume += k_volumeSteps;
            yield return new WaitForEndOfFrame();
        }
    }

    private void StopAudioOfCurrentMusic(MusicName musicToPlay)
    {
        if (musicToPlay == MusicName.intro)
        {
            m_gameplaySource.Stop();
            m_gameplaySource.enabled = false;
        }
        else
        {
            m_introSource.Stop();
            m_introSource.enabled = false;
        }
    }
}