using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class InfogameLevel
{
    public int NumberLever;
    public GameObject GameObjectLevel;
    public int ScoreMax;
}
public class GameManager : Singleton<GameManager>
{
    public int MaxCurrenScore; 
    public List<InfogameLevel> ListGameLever;
    int CurrenNumberLevel = 1;
    public GameObject CurrenLevel;
    public Block BlockData;
    GamePlayManager gamePlayManger;
    HomeUIControl HomeUIControl;
    gamePlayUiManager gamePlayUiManager;
    ObjectBoolingControler BoolingControler;
    private void Start()
    {
        BoolingControler = ObjectBoolingControler.Instance;
        gamePlayUiManager =  gamePlayUiManager.Instance;
        MaxCurrenScore = ListGameLever[0].ScoreMax;
        gamePlayManger = GamePlayManager.Instance;
        gamePlayUiManager.ChangeScore(gamePlayManger.CurrenScore.ToString()+"/"+MaxCurrenScore.ToString());
      //  StartCoroutine(DelayActiveBlockList());
    }
    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        gamePlayManger.RandomSpawnBlockChild();
        gamePlayManger.setColliderSize();
    }
    public void SetUpLevel(int Number )
    {
       
        foreach (var i in ListGameLever) {
            if(i.NumberLever == Number -1)
            {
                i.GameObjectLevel.transform.position = new Vector3(0, -20, 0);
                foreach (Transform j in i.GameObjectLevel.transform) {
                    if (j.childCount == 0) continue;
                    List<Transform> ListGameTransForm = new List<Transform>();
                     foreach(Transform k in j.transform)
                    {
                        ListGameTransForm.Add(k);
                        k.gameObject.SetActive(false);
                    }
                    BoolingControler.ObjectBack(ListGameTransForm);

                }
            }
            if (i.NumberLever == Number) {
               
                CurrenLevel = i.GameObjectLevel;
                CurrenLevel.transform.position = CurrenLevel.GetComponent<InitGrid>().DefaultCenter;
                CurrenLevel.GetComponent<InitGrid>().StartInitGrid();
                gamePlayManger.MapGamePlay = CurrenLevel;
                MaxCurrenScore = i.ScoreMax;
                gamePlayManger.CurrenScore = 0;
                gamePlayUiManager.ChangeScore(gamePlayManger.CurrenScore.ToString() + "/" + MaxCurrenScore.ToString());
                gamePlayManger.SetUpChangeLevel();
                break;
            }
        }
    }
    public void StartLever(int NumberLever)
    {
        foreach(var i in ListGameLever)
        {
            if (i.NumberLever == NumberLever) {

                i.GameObjectLevel.transform.position = i.GameObjectLevel.GetComponent<InitGrid>().DefaultCenter;
                CurrenLevel.transform.position = CurrenLevel.transform.position + new Vector3(0,-30,0);
            }
        }
    }
    public void Winlevel()
    {
        gamePlayUiManager.SetWinUI(true);
    }
    public void NextLevel()
    {

        gamePlayUiManager.SetWinUI(false);
        CurrenNumberLevel++;
        SetUpLevel(CurrenNumberLevel);
       
    }
}
