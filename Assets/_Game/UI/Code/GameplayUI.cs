using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : UICanvas
{
    [SerializeField] Image FillScore;
    [SerializeField] TextMeshProUGUI TextScore;
    [SerializeField] Button SettingButton;
    [SerializeField] Button ReRollButton;
    private void Awake()
    {
        SettingButton.onClick.AddListener(SettingEvent);
        ReRollButton.onClick.AddListener(ReRollButtonEvent);
    }
    public void SetFillScore(float CurrenScore, float MaxScore)
    {
        FillScore.fillAmount = CurrenScore/MaxScore;
    }
    public void SetTextScore(string TextValue)
    {
        TextScore.text = TextValue;
    }
     public void SettingEvent()
    {
        Close(0f);
        UIManager.Ins.GetUI<SettingUi>().Open();
    }
    public override void Open()
    {

        base.Open();
    }
    public void ReRollButtonEvent()
    {
        GameManager.Ins.Rerool();
    }
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
   
}
