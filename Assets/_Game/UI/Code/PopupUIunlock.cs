using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupUIunlock : UICanvas
{
    public Button YesButton;
    public Button NoButton;
    public Button CloseButton;

    private void Awake()
    {
        YesButton.onClick.AddListener(StartUnlock);
        CloseButton.onClick.AddListener(CloseUI);
        NoButton.onClick.AddListener(CloseUI);
    }
    public override void Open()
    {
        
        GamePlayManager.Ins.SetPause(true);
        base.Open();
        UIManager.Ins.GetUI<GameplayUI>().Close(0f);
    }
    public void StartUnlock()
    {
        CloseUI();
        GamePlayManager.Ins.UnLockEvent();
        
    }
    public void CloseUI()
    {
        Close(0f);
        GamePlayManager.Ins.SetPause(false);
        UIManager.Ins.GetUI<GameplayUI>().Open();
    }
    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
}
