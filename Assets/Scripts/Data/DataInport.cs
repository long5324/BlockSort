using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInport : Singleton<DataInport>
{
    public ObjectBoolingControler Booling;
    public AudioControl audioControl;
    public AnimationControl animationControl;
    public GamePlayManager gamePlayManager;
    public GameManager gameManager;
    public ObjectBoolingControler ObjectBooling;
    public Animation animation;
    private void Awake()
    {
        Booling = ObjectBoolingControler.Ins;
        audioControl = AudioControl.Ins;
        animationControl = AnimationControl.Ins;
        gamePlayManager = GamePlayManager.Ins;
        gameManager = GameManager.Ins;
        ObjectBooling = ObjectBoolingControler.Ins;
        animation = Animation.Ins;
    }
}
