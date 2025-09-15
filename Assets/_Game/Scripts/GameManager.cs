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
    public LevelReward LevelRewards;
}
public class GameManager : Singleton<GameManager>
{
    public int MaxCurrenScore; 
    public List<InfogameLevel> ListGameLever;
    public int CurrenNumberLevel { get; set; } = 1;
    public GameObject CurrenLevel;
    public GameObject LevelGame;
    public Block BlockData;
    public GameObject GamePlay;
    private DataInport Data;
    public GameObject CurrenGamePlay;
    public GameObject PanelGamePlay;
    public InfogameLevel CurrenLevelData;
    public InitGrid CurrenGridLevel { get; set; }

    private void Start()
    {
        Data = DataInport.Ins;
        MaxCurrenScore = ListGameLever[0].ScoreMax;
        UIManager.Ins.OpenUI<HomeUI>();
    }
    public static void ScaleByAspectRatio(Transform target, float baseScale = 1f)
    {
        float refAspect = 1080f / 1920f; // chuẩn 9:16
        float currentAspect = (float)Screen.width / Screen.height;

        // tính độ lệch so với tỉ lệ chuẩn
        float ratio = currentAspect / refAspect;

        // bạn muốn ip12 = 0.9 thì nhân thêm hệ số
        target.localScale = Vector3.one * (baseScale * ratio);
    }


    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        Data.gamePlayManager.RandomSpawnBlockChild();
        Data.gamePlayManager.setColliderSize();
    }
    public void SetUpLevel(int Number )
    {
       
        AnimationControl.Ins.ResetStateAnimationControl();
        foreach (var i in ListGameLever)
        {
            if(i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevel = Instantiate(i.GameObjectLevel, Vector3.zero, Quaternion.identity);
                ScaleByAspectRatio(CurrenLevel.transform);
                CurrenGamePlay = Instantiate(GamePlay, Vector3.zero, Quaternion.identity);
                CurrenLevel.transform.SetParent(LevelGame.transform, false);
                CurrenGamePlay.transform.SetParent(LevelGame.transform, false);
                List<GameObject> ListBlockGamePlay= new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in CurrenGamePlay.transform)
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
        Animation.Ins.SetupTransformLevelGrid(() =>
        {
           
            StartCoroutine(WaitEffectBlockChild());
        });
        
    }
    public void OpenUiGamePlay()
    {
        
        string NumberLevelName = "Level " + CurrenNumberLevel.ToString();
        UIManager.Ins.GetUI<GameplayUI>().SetupLevel(NumberLevelName, MaxCurrenScore.ToString());
        UIManager.Ins.GetUI<GameplayUI>().Open();
        UIManager.Ins.GetUI<GameplayUI>().StartIntro();
    }
    private IEnumerator WaitEffectBlockChild()
    {
            yield return new WaitForSeconds(0.3f);
        Animation.Ins.EffectBlockChildTransition(() =>
        {
            
            OpenUiGamePlay();
        });
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
                CurrenGamePlay = Instantiate(GamePlay, Vector3.zero, Quaternion.identity);
                ScaleByAspectRatio(CurrenLevel.transform);
                CurrenLevel.transform.SetParent(LevelGame.transform, false);
                CurrenGamePlay.transform.SetParent(LevelGame.transform, false);
                List<GameObject> ListBlockGamePlay = new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in CurrenGamePlay.transform)
                {
                    ListBlockGamePlay.Add(j.gameObject);
                    DefaulP.Add(j.position);
                }
                Data.gamePlayManager.ListDefaulPossitionBlockGamePlay = DefaulP;
                Data.gamePlayManager.BottomBlock = CurrenLevel.GetComponent<InitGrid>().ListblockGround;
                Data.gamePlayManager.ListBlockGamePlay = ListBlockGamePlay;
                Data.gamePlayManager.RandomSpawnBlockChild();
                CurrenGridLevel = CurrenLevel.GetComponent<InitGrid>();
                break;
            }
        }
        UpdateScore();
        CurrenGridLevel = CurrenLevel.GetComponent<InitGrid>();
        Animation.Ins.SetupTransformLevelGrid(() =>
        {
            OpenUiGamePlay();
        });
    }
    public void Reroll()
    {
        // Lấy ObjectGamePlay
        GameObject ObjectGamePlay = LevelGame.transform.GetChild(1).gameObject;
        GamePlayManager.Ins.SetPause(true);
        // 1️⃣ Dịch xuống y = -5
        ObjectGamePlay.transform.DOMoveY(-5f, 0.3f).OnComplete(() =>
        {
            // 2️⃣ Xử lý xóa child và clear data
            foreach (Transform i in ObjectGamePlay.transform)
            {
                ObjectSet OBS = i.GetComponent<ObjectSet>();
                if (OBS != null)
                    OBS.ListChildBlock.Clear();

                List<GameObject> children = new List<GameObject>();
                foreach (Transform j in i)
                {
                    children.Add(j.gameObject);
                }
                foreach (var child in children)
                {
                    LeanPool.Despawn(child);
                }
            }

            // 3️⃣ Spawn block mới
            GamePlayManager.Ins.RandomSpawnBlockChild();

            // 4️⃣ Dịch ObjectGamePlay về y = 0
            ObjectGamePlay.transform.DOMoveY(0f, 0.3f).SetEase(Ease.OutBack).OnComplete(() => {
                GamePlayManager.Ins.SetPause(false);
            });
        });
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
                ObjectR =  Instantiate(BlockData.BlockPrefab);
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
