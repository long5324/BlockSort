using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using Lean.Pool;
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
    public GameObject LevelGame;
    public Block BlockData;
    public GameObject GamePlay;
    private DataInport Data;
    public InitGrid CurrenGridLevel { get; set; }
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
        AnimationControl.Ins.ResetStateAnimationControl();
        foreach (var i in ListGameLever)
        {
            if(i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevel = Instantiate(i.GameObjectLevel, Vector3.zero, Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, Vector3.zero, Quaternion.identity);
                CurrenLevel.transform.SetParent(LevelGame.transform, false);
                GamePlayy.transform.SetParent(LevelGame.transform, false);
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
        GamePlayManager.Ins.UpdateListBlockLock();
        CurrenGridLevel = CurrenLevel.GetComponent<InitGrid>();
    }

    public void BackToHome()
    {
        foreach (Transform child in LevelGame.transform)
        {
            Destroy(child.gameObject);
        }
        CurrenLevel = new GameObject();
    }
    public void DeleteLevel()
    {
        foreach (Transform child in LevelGame.transform)
        {
            Destroy(child.gameObject);
        }
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

        foreach (Transform child in LevelGame.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var i in ListGameLever)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                CurrenLevel = Instantiate(i.GameObjectLevel, Vector3.zero, Quaternion.identity);
                GameObject GamePlayy = Instantiate(GamePlay, Vector3.zero, Quaternion.identity);

                CurrenLevel.transform.SetParent(LevelGame.transform, false);
                GamePlayy.transform.SetParent(LevelGame.transform, false);
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
    public void Rerool()
    {
        GameObject ObjectGamePlay = LevelGame.transform.GetChild(1).gameObject;

        foreach (Transform i in ObjectGamePlay.transform)
        {
            ObjectSet OBS = i.GetComponent<ObjectSet>();
            OBS.ListChildBlock.Clear();

            // ✅ Tạo list tạm để lưu con của i
            List<GameObject> children = new List<GameObject>();
            foreach (Transform j in i)
            {
                children.Add(j.gameObject);
            }

            // ✅ Despawn tất cả
            foreach (var child in children)
            {
                LeanPool.Despawn(child);
            }
        }

        GamePlayManager.Ins.RandomSpawnBlockChild();
    }
    public ChildBlock SpawnBlockChild(BlockColor Color)
    {
        ChildBlock ObjectR = null;
        foreach (BlockData i in BlockData.BlockDataBase)
        {
            if (i.Color == Color)
            {
                ChildBlock thBlock  = LeanPool.Spawn(BlockData.BlockPrefab);
                ObjectR = thBlock;
                ObjectR.Configure(i);
                break;
            }
        }
        return ObjectR;
    }
    
    public ChildBlock SpawnBlockNotBool(BlockColor Color)
    {
        ChildBlock ObjectR = null;

        foreach (BlockData b in BlockData.BlockDataBase)
        {
            if (b.Color == Color)
            {
                ObjectR = Instantiate(BlockData.BlockPrefab);
                ObjectR.Configure(b);
                break;
            }
        }
        return ObjectR;
    }
    public void DestroyLever()
    {
        foreach (Transform child in LevelGame.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void StartLever(int NumberLever)
    {
        foreach(var i in ListGameLever)
        {
            if (i.NumberLever == NumberLever) {

                i.GameObjectLevel.transform.position = Vector3.zero;
                CurrenLevel.transform.position = CurrenLevel.transform.position + new Vector3(0,-30,0);
            }
            break;
        }
    }
    public bool CheckEndGame()
    {
        if(Data.gamePlayManager.BottomBlock == null) return false;
        foreach (var i in Data.gamePlayManager.BottomBlock)
        {
             if(i.ListChildBlock.Count == 0)
            {
                return false;
            }
        }
        return true;
    }
    
    public void EventEndGame()
    {
        if (!CheckEndGame()) return;
        UIManager.Ins.GetUI<LoseUI>().Open();
        UIManager.Ins.GetUI<GameplayUI>().Close(0f);
        DestroyLever();
    }
   public void Winlevel()
    {
        Data.gamePlayManager.SetPause(true);
        UIManager.Ins.GetUI<VictoryUI>().Open();
        UIManager.Ins.GetUI<GameplayUI>().Close(0f);
    }
    public void NextLevel()
    {
        if (CurrenNumberLevel + 1 > ListGameLever.Count) return;
        StopAllAnimations();
       
        foreach (Transform child in LevelGame.transform)
        {
            Destroy(child.gameObject);
        }
        SetUpLevel(CurrenNumberLevel + 1);
       // Data.gamePlayManager.SetPause(false);
        UpdateScore();
    }

}
