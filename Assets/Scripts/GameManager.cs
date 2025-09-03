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
    public GameObject LeverGame;
    public Block BlockData;
    public GameObject GamePlay;
    DataInport Data;
    private void Start()
    {
        Data = DataInport.Ins;
     
        MaxCurrenScore = ListGameLever[0].ScoreMax;
        UIManager.Ins.OpenUI<HomeUI>();
    }

    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        Data.gamePlayManager.RandomSpawnBlockChild();
        Data.gamePlayManager.setColliderSize();
    }
    public void SetUpLevel(int Number )
    {
        UIManager.Ins.OpenUI<GameplayUI>(); 

         foreach (var i in ListGameLever)
        {
            if(i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevel = Instantiate(i.GameObjectLevel, new Vector3(5.5f, -5,1f), Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, new Vector3(1f, -0.5f, -3.5f), Quaternion.identity);

                // ✅ Đặt parent đúng cách
                CurrenLevel.transform.SetParent(LeverGame.transform, false);
                GamePlayy.transform.SetParent(LeverGame.transform, false);
                List<GameObject> ListBlockGamePlay= new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in GamePlayy.transform)
                {
                    ListBlockGamePlay.Add(j.gameObject);
                    DefaulP.Add(j.position);
                }
                Data.gamePlayManager.ListDefaulPossitionBlockGamePlay = DefaulP;
                Data.gamePlayManager.BottomBlock = CurrenLevel.GetComponent<InitGrid>().ListblockGround;
                Data.gamePlayManager.ListBlockGamePlay = ListBlockGamePlay;
                Data.gamePlayManager.RandomSpawnBlockChild();
              
                break;
            }
        }
        UpdateScore();
    }
    public void BackToHome()
    {
        CurrenLevel = new GameObject();
    }
    void UpdateScore()
    {
        Data.gamePlayManager.CurrenScore = 0;
        UIManager.Ins.GetUI<GameplayUI>().SetFillScore(0, 10);
        foreach (var i in ListGameLever)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                MaxCurrenScore = i.ScoreMax;
                break;
            }
        }
        UIManager.Ins.GetUI<GameplayUI>().SetTextScore(Data.gamePlayManager.CurrenScore.ToString() + "/" + MaxCurrenScore.ToString());
    }
    public void StopAllAnimations()
    {
        StopAllCoroutines();
        DOTween.KillAll();
        Data.animationControl.IsRun = false;
    }
    public void Replay()
    {
        StopAllAnimations();
     
        foreach (Transform child in LeverGame.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var i in ListGameLever)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                CurrenLevel = Instantiate(i.GameObjectLevel, new Vector3(5.5f, -5, 1f), Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, new Vector3(1f, -0.5f, -3.5f), Quaternion.identity);

                CurrenLevel.transform.SetParent(LeverGame.transform, false);
                GamePlayy.transform.SetParent(LeverGame.transform, false);
                List<GameObject> ListBlockGamePlay = new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in GamePlayy.transform)
                {
                    ListBlockGamePlay.Add(j.gameObject);
                    DefaulP.Add(j.position);
                }
                Data.gamePlayManager.ListDefaulPossitionBlockGamePlay = DefaulP;
                Data.gamePlayManager.BottomBlock = CurrenLevel.GetComponent<InitGrid>().ListblockGround;
                Data.gamePlayManager.ListBlockGamePlay = ListBlockGamePlay;
                Data.gamePlayManager.RandomSpawnBlockChild();
                break;
            }
        }
        UpdateScore();
    }
   public void DestroyLever()
    {
        foreach (Transform child in LeverGame.transform)
        {
            Debug.Log(1);
            Destroy(child.gameObject);
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
            break;
        }
    }
    public void Winlevel()
    {
        Data.gamePlayManager. pausegame = true;
    }
    public void NextLevel()
    {
        if (CurrenNumberLevel + 1 > ListGameLever.Count) return;
        StopAllAnimations();
       
        foreach (Transform child in LeverGame.transform)
        {
            Destroy(child.gameObject);
        }
        SetUpLevel(CurrenNumberLevel + 1);
        Data.gamePlayManager.SetPause(false);
        UpdateScore();
    }

}
