using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class HomeUIControl : Singleton<HomeUIControl>
{
    public GameObject LevelGameP;
    public int DefauGameLevel = 1;
    private List<RectTransform> ListLevel = new List<RectTransform>();
    public GameObject CanvaMainHome;
    public TextMeshProUGUI TextLevelGame;
    GamePlayManager gamePlayManager;
    gamePlayUiManager gameUI;
    GameManager gameManager;
    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        gameManager = GameManager.Instance;
        gameUI = gamePlayUiManager.Instance;    
        foreach (Transform i in LevelGameP.transform)
        {
            ListLevel.Add(i.GetComponent<RectTransform>());
        }
        ListLevel.RemoveAt(ListLevel.Count - 1);
        UpdateLevelPositionAndScale();
    }
    public void ChangeLevel(bool right)
    {
        int direction = right ? 1 : -1;

        // Cập nhật level hiện tại
        DefauGameLevel += direction;

        DefauGameLevel = Mathf.Clamp(DefauGameLevel, 0, ListLevel.Count - 1);
        SetTextLevelGame(DefauGameLevel + 1);

        UpdateLevelPositionAndScale();
    }
    public void PlayGame()
    {
        List<CanvasGroup> ListUI = new List<CanvasGroup>();
        foreach(Transform i in CanvaMainHome.transform)
        {
            ListUI.Add(i.GetComponent<CanvasGroup>());
            i.GetComponent<RectTransform>().DOLocalMove(new Vector3(0,5000,0), 1f);
        }

    }
    public void BackToHome()
    {
        gameUI.SetGamePlayUi(false);
        gameUI.SetSettingUi(false);
        gameUI.SetWinUI(false);
        gameManager.DestroyLever();
        gamePlayManager.SetPause(false);
        List<CanvasGroup> ListUI = new List<CanvasGroup>();
        foreach (Transform i in CanvaMainHome.transform)
        {
            ListUI.Add(i.GetComponent<CanvasGroup>());
            i.GetComponent<RectTransform>().DOLocalMove(Vector3.zero, 1f);
           
        }
    }

    IEnumerator WaitOffUI(List<CanvasGroup> ListUI)
    {
        yield return new WaitForSeconds(1);
        foreach (CanvasGroup i in ListUI)
        {
            i.alpha = 0;
        }
    }
  
    private void UpdateLevelPositionAndScale()
    {
        for (int i = 0; i < ListLevel.Count; i++)
        {
            int offset = i - DefauGameLevel;
         
            Vector3 targetPos = new Vector3(offset * 1100, 0, 0);

            ListLevel[i].DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutCubic);

            if (offset == 0)
            {
                ListLevel[i].DOScale(Vector3.one, 0.5f);
            }
            else if (Mathf.Abs(offset) == 1)
            {
                ListLevel[i].DOScale(Vector3.one * 0.7f, 0.5f);
            }
            else
            {
                ListLevel[i].DOScale(Vector3.one * 0.5f, 0.5f);
            }
        }
    }
    public void SetTextLevelGame(int NumberGame)
    {
        TextLevelGame.text = "LEVEL " + NumberGame.ToString();
    }
}
