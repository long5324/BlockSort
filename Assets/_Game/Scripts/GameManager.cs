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
    public  GameObject GamePlay;
    private DataInport Data;
    public GameObject CurrenGamePlay;
    public GameObject PanelGamePlay;
    public InfogameLevel CurrenLevelData;
    public InitGrid CurrenGridLevel { get; set; }
    public RectTransform MainCanvasRect;

    private void Start()
    {
        Data = DataInport.Ins;
        MaxCurrenScore = ListGameLever[0].ScoreMax;
        UIManager.Ins.OpenUI<HomeUI>();
    }
    public static void ScaleParentToFitScreen(Transform parent, Camera cam, float baseScale = 1f)
    {
        if (parent == null || cam == null) return;

        // Reset scale trước để đo bounds chính xác
        parent.localScale = Vector3.one * baseScale;

        // Gom tất cả renderer con
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var rend in renderers)
            bounds.Encapsulate(rend.bounds);

        // Tính ra các điểm góc của bounds
        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.min;
        corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[4] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[7] = bounds.max;

        // Tìm scale nhỏ nhất để toàn bộ góc nằm trong màn hình
        float minScale = float.MaxValue;
        foreach (var c in corners)
        {
            Vector3 vp = cam.WorldToViewportPoint(c);
            if (vp.z < 0) continue; // nằm sau camera thì bỏ qua

            float sx = (vp.x > 1) ? 1f / vp.x : (vp.x < 0) ? (0f - vp.x) / vp.x : 1f;
            float sy = (vp.y > 1) ? 1f / vp.y : (vp.y < 0) ? (0f - vp.y) / vp.y : 1f;

            float safeScale = Mathf.Min(sx, sy);
            if (safeScale < minScale)
                minScale = safeScale;
        }

        if (minScale < 1f && minScale > 0f)
        {
            parent.localScale *= minScale; // scale toàn bộ object cha
        }
    }


    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        Data.gamePlayManager.RandomSpawnBlockChild();
        Data.gamePlayManager.setColliderSize();
    }
    public void SetUpLevel(int Number )
    {
        GamePlayManager.Ins.SetPause(true);
        StopAllAnimations();
        if(LevelGame.transform.childCount > 0)
        {
            foreach (Transform child in LevelGame.transform)
            {
                Destroy(child.gameObject);
            }
        }
        AnimationControl.Ins.ResetStateAnimationControl();
        foreach (var i in ListGameLever)
        {
            if(i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevel = Instantiate(i.GameObjectLevel, Vector3.zero, Quaternion.identity);
                
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
        ScaleParentToFitScreen(LevelGame.transform, Camera.main, 1f);
    }
    public void OpenUiGamePlay()
    {
        
        string NumberLevelName = "Level " + CurrenNumberLevel.ToString();
        UIManager.Ins.GetUI<GameplayUI>().SetupLevel(NumberLevelName, MaxCurrenScore.ToString());
        UIManager.Ins.GetUI<GameplayUI>().Open();
        UIManager.Ins.GetUI<GameplayUI>().StartIntro();
        UIManager.Ins.GetUI<GameplayUI>().UnClickCButton();
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
        UIManager.Ins.GetUI<GameplayUI>().SetScore(0, 10);
        foreach (var i in ListGameLever)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                MaxCurrenScore = i.ScoreMax;
                break;
            }
        }
        UIManager.Ins.GetUI<GameplayUI>().SetScore(Data.gamePlayManager.CurrenScore, MaxCurrenScore);
    }
    public void StopAllAnimations()
    {
        StopAllCoroutines();
        Data.animationControl.IsRun = false;
    }

    public void Replay()
    {
        SetUpLevel(CurrenNumberLevel );
       
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
        UpdateScore();
    }

}
