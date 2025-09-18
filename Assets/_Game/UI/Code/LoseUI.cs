using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoseUI : UICanvas
{

    [SerializeField] TextMeshProUGUI TextSalceScore ;
    [SerializeField] TextMeshProUGUI TextNumberLevel;
    [SerializeField] Slider SliderScore;
    [SerializeField] Button ButtomReplay;
    [SerializeField] Button ButtomBachHome;
    private void Awake()
    {
        ButtomReplay.onClick.AddListener(Replay);
        ButtomBachHome.onClick.AddListener(GoBackHome);
    }
    void Replay()
    {
        GameManager.Ins.Replay();
        Close(0f);
        GamePlayManager.Ins.SetPause(false);
    }
    public override void Open()
    {
        SetUpDataLoseGame();
        GamePlayManager.Ins.SetPause(true);
        base.Open();
    }
    
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }

    void GoBackHome()
    {
        GameManager.Ins.BackToHome();
        Close(0f);
        UIManager.Ins.GetUI<HomeUI>().Open();
        GamePlayManager.Ins.SetPause(false);
    }
    public void SetUpDataLoseGame() {
        TextSalceScore.text = GamePlayManager.Ins.CurrenScore.ToString() + "/" + GameManager.Ins.MaxCurrenScore.ToString();
        SliderScore.value = (float)GamePlayManager.Ins.CurrenScore / (float)GameManager.Ins.MaxCurrenScore;
    }
}
