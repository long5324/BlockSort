using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gamePlayUiManager : Singleton<gamePlayUiManager>
{
    public CanvasGroup MainUI;
    public GameplayUI gamePlayUI;
    public void ChangeScore(string text)
    {
        gamePlayUI.SetTextScore(text);
    }
    public void Setfill(float currenScore, float MaxScore)
    {
        gamePlayUI.SetFillScore(currenScore, MaxScore);
    }

}
