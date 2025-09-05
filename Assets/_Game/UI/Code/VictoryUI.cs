using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryUI : UICanvas
{
    [SerializeField] Button RePlayButton;
    [SerializeField] Button NextLevelButton;
    [SerializeField] Button HomeButton;
    GameplayUI GamePlayUI;
    private void Start()
    {
        RePlayButton.onClick.AddListener(Replay);
        NextLevelButton.onClick.AddListener(NextLevel);
        HomeButton.onClick.AddListener(GoBackHome);
        GamePlayUI = UIManager.Ins.GetUI<GameplayUI>();
    }
    public override void Open()
    {
        GameManager.Ins.DeleteLevel();
        base.Open();
    }
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
    void Replay()
    {
        GameManager.Ins.Replay();
        Close(0f);
        GamePlayUI.Open();
        GamePlayManager.Ins.SetPause(false);    
    }
    void NextLevel()
    {
        GameManager.Ins.NextLevel();
        Close(0f);
        GamePlayUI.Open();
        GamePlayManager.Ins.SetPause(false);
    }
    void GoBackHome()
    {
        GameManager.Ins.BackToHome();
        Close(0f);
        UIManager.Ins.GetUI<HomeUI>().Open();
        GamePlayManager.Ins.SetPause(false);
    }
}
