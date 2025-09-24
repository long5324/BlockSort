using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HammerAnimationEvent : MonoBehaviour
{
    public UnityEvent EventHitHammer;
    public UnityEvent EventEndHammerAnimation;
    public void StartEvent()
    {
        EventHitHammer.Invoke();
        AudioControl.Ins.PlaySFX("HammerEffectSound");
    }
    public void EndAnimation()
    {
        EventEndHammerAnimation.Invoke();
    }
}
