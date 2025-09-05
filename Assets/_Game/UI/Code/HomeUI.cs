using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button playBtn;
    [SerializeField] Button RightButton;
    [SerializeField] Button LeftButton;
    public List<RectTransform> ListLevel = new List<RectTransform>();
    public DataHomeLevel Data;
    public int NumberLevel;
    IEnumerator WaitOffUI(List<CanvasGroup> ListUI)
    {
        yield return new WaitForSeconds(1);
        foreach (CanvasGroup i in ListUI)
        {
            i.alpha = 0;
        }
    }
    private void Awake()
    {
        playBtn.onClick.AddListener(StartGame);
        for(int i =0; i< ListLevel.Count; i++)
        {
            ListLevel[i].GetComponent<Image>().sprite = Data.UIGameLevel[i];
        }
    }
    public void EventRightButton()
    {
    }
    public override void Open()
    {
        base.Open();
    }


    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
    void StartGame()
    {
        Close(0f);
        GameManager.Ins.SetUpLevel(1);
    }
}
