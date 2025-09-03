using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button playBtn;
    private List<RectTransform> ListLevel = new List<RectTransform>();
    public DataHomeLevel Data;
    public List<GameObject> ObjectLevel;
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
