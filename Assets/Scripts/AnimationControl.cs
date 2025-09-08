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
    Coroutine DeLayCheckScore=null;
    private void Start()
    {
        Data = DataInport.Ins;
        gamePlayManager = GamePlayManager.Ins;
    }

    private void Update()
    {

        if(IsRun && Ani.BlockStart == null)
        {
            IsRun = false;
        }

        if (Data.gamePlayManager.DelayCheck.Count==0 && Ani.BlockStart == null && !IsRun && !ScorePlus && Data.gameManager.CheckEndGame())
        {
            Data.gameManager.EventEndGame();
        }
        if (ScorePlus)
        {
            delaysort = true;
        }
        else delaysort = false; 
        if (Ani.BlockStart!=null && !IsRun && !ScorePlus && !delaysort )
        {
            HandleData();
            IsRun = true;
            if (DeLayCheckScore != null)
            {
                StopCoroutine(DeLayCheckScore);
                DeLayCheckScore = null;
            }
            
        }
        else if (Ani.BlockStart == null && !IsRun && !ScorePlus && DeLayCheckScore == null)
        {
            DeLayCheckScore= StartCoroutine(DelayCheckSocre());
            
        }

    }
    public IEnumerator DelayCheckSocre()
    {
        yield return new WaitForSeconds(0.05f);
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
        int MaxScore = 0;
        bool Check = false;
        foreach (var block in gamePlayManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gamePlayManager.MunberBlockEat || gamePlayManager.CheckScore(block) < gamePlayManager.MunberBlockEat)
                continue;

            int score = gamePlayManager.CheckScore(block);
            if(MaxScore < score)
            {
                MaxScore = score;
            }
        }
            foreach (var block in gamePlayManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gamePlayManager.MunberBlockEat || gamePlayManager.CheckScore(block) < gamePlayManager.MunberBlockEat)
                continue;

            int score = gamePlayManager.CheckScore(block);
            if (score > 0)
            {
                gamePlayManager.ScorePluss += score;
                if (score == MaxScore && !Check )
                {
                    Check = true;
                    StartCoroutine(Data.animation.PlusScore(block, score, 0, true));
                }
                else 
                {
                    StartCoroutine(Data.animation.PlusScore(block, score, 0, false));
                }
                    ScorePlus = true;
            }

        }
    }
    public void AddAni(BlockControl Start, BlockControl End )
    {
        if (Start == null || End == null)
        {
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
            // Kiểm tra i có bị destroy không
            if (i == null) continue;

            if (i.PosionBlock == Po)
            {
                i.ListChildBlock = new List<ChildBlock>();

                foreach (Transform k in i.transform)
                {
                    var child = k.GetComponent<ChildBlock>();
                    if (child != null)
                    {
                        i.ListChildBlock.Add(child);
                    }
                }
            }
        }

        Ani = new IfData();
    }



}
