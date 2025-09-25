using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


public class GameManager : Singleton<GameManager>
{
    [Header("Data Import")]
    public GameLevelData GameLevelDataBase;
    public Block BlockData;
    public GameObject LevelGame;
    public GameObject GamePlayPrefab;
    [Header("Other")]
    public GameObject PanelGamePlay;
    public RectTransform MainCanvasRect;
    public Animator AnimationTransition;
    public TextMeshProUGUI TextLevelTransition;
    public ParticleSystem EffectSelectBlock;
    public ParticleSystem ParticleEffectSetBlock;
    public ParticleSystem ParticleEffectHammer;
    public Animator HammerAnimator;
    public GameObject CurrenGamePlay { get; private set; }
    public InfoGameLevel CurrenLevelData { get; private set; }
    public GameObject CurrenLevelGameObject { get; private set; }
    public InitGrid CurrenGridLevel { get; private set; }
    public int MaxCurrenScore { get; private set; }
    public DragRotate RotateLevel { get; private set; }
    public int CurrenNumberLevel { get; private set; } = 1;

    private void Start()
    {
        MaxCurrenScore = GameLevelDataBase.ListGameLevel[0].ScoreMax;
        UIManager.Ins.OpenUI<HomeUI>();
    }
    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(1);
        GamePlayManager.Ins.RandomSpawnBlockChild();
        GamePlayManager.Ins.setColliderSize();
    }
    public void SetUpLevel(int Number )
    {
        UIManager.Ins.GetUI<HomeUI>().Close(1);
        AnimationTransition.SetTrigger("Close");
        TextLevelTransition.text = "Level " + Number.ToString();
        StartCoroutine(StartInit(Number));
    }
    public IEnumerator  StartInit(int Number)
    {
        yield return new WaitForSeconds(1);
        UIManager.Ins.GetUI<GameplayUI>().Close(0f);
        //pause Game And Kill Animation
        GamePlayManager.Ins.SetPause(true);
        StopAllAnimations();
        AnimationControl.Ins.ResetStateAnimationControl();
        //Delete Last Level 
        if (LevelGame.transform.childCount > 0)
        {
            foreach (Transform child in LevelGame.transform)
            {
                Destroy(child.gameObject);
            }
        }
        //Init new Level Gameobject 
        foreach (var i in GameLevelDataBase.ListGameLevel)
        {
            if (i.NumberLever == Number)
            {
                CurrenNumberLevel = Number;
                CurrenLevelGameObject = Instantiate(i.GameObjectLevel, Vector3.zero, Quaternion.identity);
                CurrenGamePlay = Instantiate(GamePlayPrefab, Vector3.zero, Quaternion.identity);
                CurrenLevelGameObject.transform.SetParent(LevelGame.transform, false);
                CurrenGamePlay.transform.SetParent(LevelGame.transform, false);
                List<GameObject> ListBlockGamePlay = new List<GameObject>();
                List<Vector3> DefaulP = new List<Vector3>();
                foreach (Transform j in CurrenGamePlay.transform)
                {
                    ListBlockGamePlay.Add(j.gameObject);
                    DefaulP.Add(j.position);
                }
                GamePlayManager.Ins.ListDefaulPossitionBlockGamePlay = DefaulP;
                GamePlayManager.Ins.BottomBlock = CurrenLevelGameObject.GetComponent<InitGrid>().ListblockGround;
                GamePlayManager.Ins.ListBlockGamePlay = ListBlockGamePlay;
                GamePlayManager.Ins.RandomSpawnBlockChild();
                break;
            }
        }
        UpdateScore();
        GamePlayManager.Ins.UpdateListBlockLock();
        CurrenGridLevel = CurrenLevelGameObject.GetComponent<InitGrid>();
        RotateLevel = CurrenLevelGameObject.GetComponent<DragRotate>();
        Number = 0;
        StartCoroutine(WaitEffectBlockChild());
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
        yield return new WaitForSeconds(1.5f);
        OpenUiGamePlay();
        /*Animation.Ins.EffectBlockChildTransition(() =>
        {

            OpenUiGamePlay();
        });*/
    }
    public void BackToHome()
    {
        AnimationTransition.SetTrigger("Close");
        TextLevelTransition.text = "Back Home";
        CurrenLevelGameObject = new GameObject();
        StartCoroutine(DelayOpenHomeUI());
    }
    private IEnumerator DelayOpenHomeUI()
    {
        yield return new WaitForSeconds(1f);
        UIManager.Ins.GetUI<HomeUI>().Open();
        DeleteLevel();
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
        GamePlayManager.Ins.CurrenScore = 0;
        UIManager.Ins.GetUI<GameplayUI>().SetScore(0, 10);
        foreach (var i in GameLevelDataBase.ListGameLevel)
        {
            if (i.NumberLever == CurrenNumberLevel)
            {
                MaxCurrenScore = i.ScoreMax;
                break;
            }
        }
        UIManager.Ins.GetUI<GameplayUI>().SetScore(GamePlayManager.Ins.CurrenScore, MaxCurrenScore);
    }
    public void StopAllAnimations()
    {
        StopAllCoroutines();
        AnimationControl.Ins.IsRun = false;
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
        ObjectGamePlay.transform.DOMoveY(-5f, 0.3f).OnComplete(() =>
        {
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
            GamePlayManager.Ins.RandomSpawnBlockChild();
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
        foreach(var i in GameLevelDataBase.ListGameLevel)
        {
            if (i.NumberLever == NumberLever) {

                i.GameObjectLevel.transform.position = Vector3.zero;
                CurrenLevelGameObject.transform.position = CurrenLevelGameObject.transform.position + new Vector3(0,-30,0);
            }
            break;
        }
    }
    public bool CheckEndGame()
    {
        List<BlockControl> ListBlockControl = GamePlayManager.Ins.BottomBlock;
        if (ListBlockControl == null || AnimationControl.Ins.IsRun || AnimationControl.Ins.ScorePlus || GamePlayManager.Ins.DelayCheck.Count > 0) return false;
        Debug.Log(GamePlayManager.Ins.DelayCheck.Count);
        foreach (var i in ListBlockControl)
        {
            if ((i.ListChildBlock.Count == 0 && i.State == StateBlock.Nomal) )
            {
                return false;
            }
        }
        return true;
    }
    
    public IEnumerator EventEndGame()
    {
        yield return new WaitForSeconds(0.5f);
        if (CheckEndGame())
        {
            UIManager.Ins.GetUI<LoseUI>().Open();
            UIManager.Ins.GetUI<GameplayUI>().Close(2f);
            DestroyLever();
        }
    }
    public void Winlevel()
    {
        UIManager.Ins.GetUI<GameplayUI>().Close(2f);
        GamePlayManager.Ins.SetPause(true);
        if (LevelGame.transform.childCount == 2)
        {
            Destroy(LevelGame.transform.GetChild(1).gameObject);
        }
        StartCoroutine(WaitEffect());
        StartCoroutine(ShowVictoryUI());
    }
    private IEnumerator WaitEffect()
    {
        yield return new WaitForSeconds(1f); // đợi 1s

        Animation.Ins.AnimationVictoryGameLevel();

    }
    private IEnumerator ShowVictoryUI()
    {
        yield return new WaitForSeconds(1f); // đợi 1s

        UIManager.Ins.GetUI<VictoryUI>().Open();
       
    }

    public void NextLevel()
    {
        if (CurrenNumberLevel + 1 > GameLevelDataBase.ListGameLevel.Count) return;
        SetUpLevel(CurrenNumberLevel + 1);
    }

}
