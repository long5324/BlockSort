using UnityEngine;

public class AudioControl : Singleton<AudioControl>
{
    public AudioSource Control;
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
    
}
