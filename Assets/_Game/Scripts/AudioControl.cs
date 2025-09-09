using System.Collections.Generic;
using UnityEngine;

public class AudioControl : Singleton<AudioControl>
{
    /*public AudioSource Control;
    public Audio ListAudio;
    public void StartUp()
    {
        AudioClip Audio = ListAudio.GetSound("up");
        if (Control == null || Audio == null) return;
        Control.PlayOneShot(Audio);
    }
    public void StartDown()
    {
        AudioClip Audio = ListAudio.GetSound("down");
        if (Control == null || Audio == null) return;
        Control.PlayOneShot(Audio);
    }
    public void StartMove()
    {
        AudioClip Audio = ListAudio.GetSound("move");
        if (Control == null || Audio == null) return;
        Control.PlayOneShot(Audio);
    }
    
*/
    #region SFX
    public static string up = "up";
    public static string down = "down";
    public static string move = "move";

   
    #endregion

    #region Song   
    public static string MainSong = "MainSong";
    #endregion

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public List<AudioClip> sfxClips;
    public List<AudioClip> musicClips;

    [HideInInspector] public bool IsPlaySFX = true;
    [HideInInspector] public bool IsPlayMusic;
    [HideInInspector] public bool IsHaptic;

    public void Initialize()
    {
       
        ChangeMusicState();
        PlayMusic(MainSong);
    }

    public void PlaySFX(string sfxName)
    {
    //    if (!IsPlaySFX) return;
        foreach (AudioClip clip in sfxClips)
        {
            if (clip.name == sfxName)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!IsPlaySFX) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(string musicName)
    {
        AudioClip musicClip;
        foreach (AudioClip clip in musicClips)
        {
            if (clip.name == musicName)
            {
                musicClip = clip;
                if (musicSource.clip != musicClip)
                {
                    musicSource.clip = musicClip;
                    musicSource.Play();
                }
            }
        }
    }

    public void ActiveMusic(bool active)
    {
        if (IsPlayMusic)
        {
            musicSource.volume = active ? 1f : 0f;
        }
    }

    public void ChangeMusicState() => musicSource.volume = IsPlayMusic ? 0.2f : 0f;

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            
        }
    }

    private void OnApplicationQuit()
    {
       
    }


}

