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
    GamePlayManager gamePlayManger;
    HomeUIControl HomeUIControl;
    gamePlayUiManager gamePlayUiManager;
    ObjectBoolingControler BoolingControler;
    AnimationControl AControl;
    private void Start()
    {
        AControl = AnimationControl.Instance;
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
        
         foreach (var i in ListGameLever)
        {
            if(i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevel = Instantiate(i.GameObjectLevel, new Vector3(5.5f, -5, 1), Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, new Vector3(-2.5f, 2, -6.6f), Quaternion.identity);

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
                gamePlayManger.ListDefaulPossitionBlockGamePlay = DefaulP;
                gamePlayManger.BottomBlock = CurrenLevel.GetComponent<InitGrid>().ListblockGround;
                gamePlayManger.ListBlockGamePlay = ListBlockGamePlay;
                gamePlayManger.RandomSpawnBlockChild();
                gamePlayUiManager.SetGamePlayUi(true);
                break;
            }
        }
        UpdateScore();
    }
    void UpdateScore()
    {
        gamePlayManger.CurrenScore = 0;
        gamePlayUiManager.Setfill(0,10);
        foreach (var i in ListGameLever) {
            if(i.NumberLever == CurrenNumberLevel)
            {
                MaxCurrenScore = i.ScoreMax;
                break;
            }
        }
        gamePlayUiManager.ChangeScore(gamePlayManger.CurrenScore.ToString() + "/" + MaxCurrenScore.ToString());
    }
    public void StopAllAnimations()
    {
        // 1. Dừng toàn bộ coroutine của script Animation
        StopAllCoroutines();

        // 2. Dừng toàn bộ tween của DOTween trong game
        DOTween.KillAll();

        // 3. (Tuỳ chọn) Reset trạng thái nếu cần
        AControl.IsRun = false;
    }
    public void Replay()
    {
        StopAllAnimations();
     
        foreach (Transform child in LeverGame.transform)
        {
            Debug.Log(1);
            Destroy(child.gameObject);
        }
        foreach (var i in ListGameLever)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                CurrenLevel = Instantiate(i.GameObjectLevel, new Vector3(5.5f, -5, 1), Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, new Vector3(-2.5f, 2, -6.6f), Quaternion.identity);

                // ✅ Đặt parent đúng cách
                CurrenLevel.transform.SetParent(LeverGame.transform, false);
                GamePlayy.transform.SetParent(LeverGame.transform, false);
                List<GameObject> ListBlockGamePlay = new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in GamePlayy.transform)
                {
                    ListBlockGamePlay.Add(j.gameObject);
                    DefaulP.Add(j.position);
                }
                gamePlayManger.ListDefaulPossitionBlockGamePlay = DefaulP;
                gamePlayManger.BottomBlock = CurrenLevel.GetComponent<InitGrid>().ListblockGround;
                gamePlayManger.ListBlockGamePlay = ListBlockGamePlay;
                gamePlayManger.RandomSpawnBlockChild();
                gamePlayUiManager.SetGamePlayUi(true);
                gamePlayUiManager.SetSettingUi(false);
                gamePlayUiManager.SetWinUI(false);
                gamePlayManger.SetPause(false);
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
        gamePlayUiManager.SetWinUI(true);
        gamePlayUiManager.SetGamePlayUi(false);
        gamePlayManger. pausegame = true;
    }
    public void NextLevel()
    {
        if (CurrenNumberLevel + 1 > ListGameLever.Count) return;
        StopAllAnimations();
       
        foreach (Transform child in LeverGame.transform)
        {
            Debug.Log(1);
            Destroy(child.gameObject);
        }
        gamePlayUiManager.SetWinUI(false);
        SetUpLevel(CurrenNumberLevel + 1);
        gamePlayManger.SetPause(false);
        gamePlayUiManager.SetSettingUi(false);
        UpdateScore();
    }
    public void StartGamme()
    {
        gamePlayUiManager.SetSetting(false);
    }
    public void RePlay()
    {
        foreach (Transform i in CurrenLevel.transform)
        {
            for (int j = i.childCount - 1; j >= 0; j--)
            {
                BlockControl bc = i.GetChild(j).gameObject.GetComponent<BlockControl>();
                if(bc != null)
                    bc.ListChildBlock.Clear();
                DestroyImmediate(i.GetChild(j).gameObject);
            }
        }
        gamePlayManger.SetPause(false);
        gamePlayUiManager.SetSetting(false);
    }
}
