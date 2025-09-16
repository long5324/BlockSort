using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUi : UICanvas
{
    [SerializeField] Button CloseButton;
    [SerializeField] Button RePlayButton;
    [SerializeField] Button NextLevelButton;
    [SerializeField] Button HomeButton;
    [SerializeField] Image  FillScore;
    [SerializeField] TextMeshProUGUI TextScore;
    [SerializeField] GameObject PanelSetting;
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
        Close(0f);
        GamePlayUI.Open();
     
    }
    void Replay()
    {
        Close(0f);
        GameManager.Ins.Replay();
       
    }
    void NextLevel()
    {
        Close(0f);
        GameManager.Ins.NextLevel();
       
    }
    void GoBackHome()
    {
        Close(0f);
        GameManager.Ins.BackToHome();
       
        UIManager.Ins.GetUI<HomeUI>().Open();
        GamePlayManager.Ins.SetPause(false);
    }
    private DOTweenAnimation tweenAnim;
    public void setFill()
    {
        FillScore.fillAmount = GamePlayManager.Ins.CurrenScore / GameManager.Ins.MaxCurrenScore;
    }
    public override void Open()
    {
        base.Open();
        TextScore.text = GamePlayManager.Ins.CurrenScore.ToString() + "/" + GameManager.Ins.MaxCurrenScore.ToString();
        GamePlayManager.Ins.SetPause(true);
    }
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
        GamePlayManager.Ins.SetPause(false);
    }
    public void OpenSetting(bool b)
    {
        Open();
    }
}
