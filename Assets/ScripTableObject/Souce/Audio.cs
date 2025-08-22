using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioSouce", menuName = "Audio/ListAudio")]
public class Audio : ScriptableObject
{
    [System.Serializable]
    public class SoundInfo
    {
        public string name;
        public AudioClip Sound;
    }
    public List<SoundInfo> ListSound;
    public AudioClip GetSound(string name)
    {
        foreach (var sound in ListSound) {
            if (sound.name.Equals(name))
                return sound.Sound;
        }
        Debug.LogWarning("Sound " + name + " null");
        return null;
    }
}
