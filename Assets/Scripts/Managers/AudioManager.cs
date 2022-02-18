using System.Collections;
using UnityEngine;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public enum MusicName { intro, gameplay };

    [Header("Music")]
    [SerializeField]
    AudioSource m_introSource;

    [SerializeField]
    AudioSource m_gameplaySource;

    [SerializeField]
    [Range(0f, 1f)]
    float m_maxMusicVolume;

    [Header("SFX")]
    [SerializeField]
    AudioSource m_sfxSource;

    float m_volumeSteps = 0.01f;

    void Start()
    {
        // We now that the menu scene if the first to start
        PlayMusic(MusicName.intro);
    }
   
    // Play a sound effect with some volume default 0.8
    public void PlaySound(AudioClip clip, float volume = 0.8f)
    {
        m_sfxSource.PlayOneShot(clip);
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

    // Play the gameplay music using a cross-play effect
    public void CrossPlayGameplay()
    {        
        m_gameplaySource.volume = 0;
        m_gameplaySource.enabled = true;
        m_gameplaySource.Play();
        StartCoroutine(CrossPlayMusic(MusicName.gameplay));
    }

    // The cross-play effect process
    private IEnumerator CrossPlayMusic(MusicName musicToPlay)
    {
        float volume = 0f;

        // Repeat until the volume go up to the max
        while (volume <= m_maxMusicVolume)
        {
            if (musicToPlay == MusicName.intro)
            {
                m_introSource.volume += m_volumeSteps;
                m_gameplaySource.volume -= m_volumeSteps;
            }
            else
            {
                m_introSource.volume -= m_volumeSteps;
                m_gameplaySource.volume += m_volumeSteps;
            }
            volume += m_volumeSteps;
            yield return new WaitForEndOfFrame();
        }

        // Stop the audio of the old music
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
