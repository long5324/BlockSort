using UnityEngine;

public class AudioControl : Singleton<AudioControl>
{
    public AudioSource Control;
    public AudioClip SoundUp;
    public AudioClip SoundDown;
    public AudioClip SoundMove;
    public void StartUp()
    {
        if (Control == null || SoundUp == null) return;
        Control.PlayOneShot(SoundUp);
    }
    public void StartDown()
    {
        if (Control == null || SoundDown == null) return;
        Control.PlayOneShot(SoundDown);
    }
    public void StartMove()
    {
        if (Control == null || SoundMove == null) return;
        Control.PlayOneShot(SoundMove);
    }
}
