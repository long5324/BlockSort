using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ByBooters : UICanvas
{
    [SerializeField] Button CloseButton;
    [SerializeField] Button YesButton;
    [SerializeField] Button NoButton;
    [SerializeField] Button PlussBooters;
    [SerializeField] Button ReduceBooters;
    [SerializeField] TextMeshProUGUI NumberCoinBuy;
    [SerializeField] TextMeshProUGUI NumberBooters;
    public BootersName ButtonWaintBy { get; set; } = BootersName.none;
    public int coinBy { get; set; } = 0;
    public int NumberBootersBuy { get; set; } = 1;  
    public int coinSpend { get; set; } = 0;
    private void Awake()
    {
        CloseButton.onClick.AddListener(ClosePopup);
        NoButton.onClick.AddListener(ClosePopup);
        YesButton.onClick.AddListener(YesBuyBooters);
        PlussBooters.onClick.AddListener(PlussBootersControl);
        ReduceBooters.onClick.AddListener(ReduceBootersControl);
    }
    public void ClosePopup()
    {
        ButtonWaintBy = BootersName.none;
        coinBy = 0;
        Close(0f);
    }
    public void PlussBootersControl(){
        NumberBootersBuy++;
        UpdateCompo();
        CheckCoin();
    }
    public void ReduceBootersControl()
    {
        if(NumberBootersBuy ==1) return;
        NumberBootersBuy--;
        UpdateCompo();
        CheckCoin();
    }
    public void UpdateCompo()
    {
        NumberBooters.text = NumberBootersBuy.ToString();
        Debug.Log(coinBy + " " + NumberBootersBuy);
        coinSpend = (coinBy * NumberBootersBuy);
        NumberCoinBuy.text = coinSpend.ToString();
    }
    public void YesBuyBooters()
    {
        Currency.Ins.AddBooster(ButtonWaintBy, NumberBootersBuy);
        UIManager.Ins.GetUI<GameplayUI>().UpdateBooters();
        UIManager.Ins.GetUI<GameplayUI>().SetCoinEffect(coinSpend);
        ButtonWaintBy = BootersName.none;
        coinBy = 0;
        coinSpend = 0;
        NumberBootersBuy = 1;
        ClosePopup();
    }
    public override void Close(float delayTime)
    {
        GamePlayManager.Ins.SetPause(false);
        coinSpend = 0;
        NumberBootersBuy = 1;
        coinBy = 0;
        base.Close(delayTime);
    }
    public override void Open()
    {
        base.Open();
        StartCoroutine(WaitUpdate());
    }
    IEnumerator  WaitUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateCompo();
    }
    public void CheckCoin()
    {
        if(coinSpend > Currency.Ins.DataCurrency.coin)
        {
            NumberCoinBuy.color = Color.red;
            YesButton.interactable = false;
            return;
        }
        NumberCoinBuy.color = Color.white;
        YesButton.interactable = true;
    }
}
