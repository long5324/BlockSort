using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInport : Singleton<DataInport>
{
    public AudioControl audioControl;
    public AnimationControl animationControl;
    public GamePlayManager gamePlayManager;
    public GameManager gameManager;
    public Animation animation;
    private void Awake()
    {
        audioControl = AudioControl.Ins;
        animationControl = AnimationControl.Ins;
        gamePlayManager = GamePlayManager.Ins;
        gameManager = GameManager.Ins;
        animation = Animation.Ins;
    }
}
