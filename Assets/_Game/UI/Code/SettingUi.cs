using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class SettingUi : UICanvas
{
    [SerializeField] Button CloseButton;
    [SerializeField] Button RePlayButton;
    [SerializeField] Button NextLevelButton;
    [SerializeField] Button HomeButton;
    GameplayUI GamePlayUI;
    private void Start()
    {
        CloseButton.onClick.AddListener(CloseSetting);
        RePlayButton.onClick.AddListener(Replay);
        NextLevelButton.onClick.AddListener(NextLevel);
        HomeButton.onClick.AddListener(GoBackHome);
        GamePlayUI = UIManager.Ins.GetUI<GameplayUI>();
    }
    void CloseSetting()
    {
        GamePlayUI.Open();
        Close(0f);
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
    public override void Open()
    {
        base.Open();
    }
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
    public void OpenSetting(bool b)
    {
        Open();
    }
}
