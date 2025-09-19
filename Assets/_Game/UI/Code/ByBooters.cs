using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ByBooters : UICanvas
{
    [SerializeField] Button CloseButton;
    [SerializeField] Button YesButton;
    [SerializeField] Button NoButton;
    public BootersName ButtonWaintBy { get; set; } = BootersName.none;
    public int coinBy { get; set; } = 0;
    private void Awake()
    {
        CloseButton.onClick.AddListener(ClosePopup);
        NoButton.onClick.AddListener(ClosePopup);
        YesButton.onClick.AddListener(YesBuyBooters);
    }
    public void ClosePopup()
    {
        ButtonWaintBy = BootersName.none;
        coinBy = 0;
        Close(0f);
    }
    public void YesBuyBooters()
    {
        Currency.Ins.AddBooster(ButtonWaintBy, 1);
        UIManager.Ins.GetUI<GameplayUI>().UpdateBooters();
        UIManager.Ins.GetUI<GameplayUI>().SetCoinEffect(coinBy);
        ButtonWaintBy = BootersName.none;
        coinBy = 0;
        ClosePopup();
    }
    public override void Close(float delayTime)
    {
        GamePlayManager.Ins.SetPause(false);
        base.Close(delayTime);
    }
    public override void Open()
    {
        base.Open();
       
    }
}
