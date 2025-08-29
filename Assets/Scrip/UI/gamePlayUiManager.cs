using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gamePlayUiManager : Singleton<gamePlayUiManager>
{
    public Image FillSocre;
    public TextMeshProUGUI TextScore;
    public List<CanvasGroup> WinUI;
    public List<CanvasGroup> SettingUi;
    public List<CanvasGroup> GamePlayUI;
    private void Start()
    {
        FillSocre.fillAmount = 0;
        
    }
    public void ChangeScore(string text)
    {
        TextScore.text = text;
    }
    public void Setfill(float currenScore, float MaxScore)
    {
        FillSocre.fillAmount = currenScore / MaxScore;
    }
    public void SetWinUI(bool t)
    {
        foreach (var i in WinUI)
        {
            if (t)
            {
                i.alpha = 1;
                i.interactable = true;
                i.blocksRaycasts = true;
            }else
            {
                i.alpha = 0;
                i.interactable = false;
                i.blocksRaycasts = false;
            }
        }
    }
    public void SetSetting(bool t)
    {
        foreach (var i in SettingUi)
        {
            if (t)
            {
                i.alpha = 1;
                i.interactable = true;
                i.blocksRaycasts = true;
            }
            else
            {
                i.alpha = 0;
                i.interactable = false;
                i.blocksRaycasts = false;
            }
        }
        foreach (var i in GamePlayUI)
        {
            if (!t)
            {
                i.alpha = 1;
                i.interactable = true;
                i.blocksRaycasts = true;
            }
            else
            {
                i.alpha = 0;
                i.interactable = false;
                i.blocksRaycasts = false;
            }
        }
    }
}
