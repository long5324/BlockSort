using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class IfData
{
    public BlockControl BlockStart;
    public BlockControl BlockEnd;
}
[System.Serializable]


public class AnimationControl : Singleton<AnimationControl>
{
  
    public bool ScorePlus { get;  set; } = false;  
    private GamePlayManager gamePlayManager;
    bool delaysort = false;
    public bool IsRun { get;  set; } = false;
    public IfData Ani = new IfData();
    DataInport Data;
    bool StratCheck = false;
    Coroutine CheckSocre = null;

    private void Start()
    {
        Data = DataInport.Ins;
        gamePlayManager = GamePlayManager.Ins;
       // GetComponent<Data.animation>().AniStartButton(uiManager.getStartButton());
    }
    private void Update()
    {
        if (ScorePlus)
        {
            delaysort = true;
        }
        else delaysort = false; 
        if (Ani.BlockStart!=null && !IsRun && !ScorePlus && !delaysort)
        {
            HandleData();
            IsRun = true;
            if (CheckSocre != null)
            {
                StopCoroutine(CheckSocre);
                CheckSocre = null;
            }
        }
        else if (Ani.BlockStart == null && !IsRun && !ScorePlus && CheckSocre == null)
        {
            CheckSocre = StartCoroutine(DelayCheckSocre());
        }

    }
    public IEnumerator DelayCheckSocre()
    {
        yield return new WaitForSeconds(1);
        HandleScore();
    }
    private void HandleData()
    {
        var firstData = Ani;

        if (firstData != null && firstData.BlockStart != null && firstData.BlockStart !=null && firstData.BlockEnd != null)
        {
            if (Ani == null)
            {
                Debug.LogWarning("Ani.IfCheck = null, không thể chạy Data.animation");
                return;
            }

            var aniComp = GetComponent<Animation>();
            if (aniComp == null)
            {
                Debug.LogError("Không tìm thấy component Data.animation trên GameObject");
                return;
            }
          
            aniComp.RunUpBlocks(Ani.BlockEnd, Ani.BlockStart);
        }
        else
        {
            Debug.LogWarning("HandleData.animations bị gọi nhưng dữ liệu Ani chưa hợp lệ");
        }
    }

    private void HandleScore()
    {
        if (gamePlayManager.BottomBlock == null || gamePlayManager.BottomBlock.Count == 0) return;
        foreach (var block in gamePlayManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gamePlayManager.MunberBlockEat || gamePlayManager.CheckScore(block) < gamePlayManager.MunberBlockEat)
                continue;

            int score = gamePlayManager.CheckScore(block);
            if (score > 0)
            {
                gamePlayManager.ScorePluss += score;
                StartCoroutine(Data.animation.PlusScore(block, score, 0));
                ScorePlus = true;
            }

        }
    }
    public void AddAni(BlockControl Start, BlockControl End )
    {
        if (Start == null || End == null)
        {
            Debug.Log(1);
            return;
        }
       
        IfData newData = new IfData
        {
            BlockStart = Start,
            BlockEnd = End,
        };
        Ani = newData;
    }
    public void ChangeInDataBlockControl(Vector3 Po)
    {
        foreach (var i in gamePlayManager.BottomBlock)
        {
            if (i.PosionBlock == Po)
            {
                i.ListChildBlock = new List<ChildBlock>();

                foreach (Transform k in i.transform)
                {
                    var child = k.GetComponent<ChildBlock>();
                    if (child != null)
                        i.ListChildBlock.Add(child);
                }
            }
        }
        Ani = new IfData();
    }


}
