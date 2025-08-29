using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using static AnimationControl;
[System.Serializable]
public class IfAnimation
{
    public BlockControl BlockStart;
    public BlockControl BlockEnd;
}
[System.Serializable]


public class AnimationControl : Singleton<AnimationControl>
{
  
    public bool ScorePlus { get;  set; } = false;  
    private GamePlayManager gamePlayManager;
    private UIManager uiManager;
    bool delaysort = false;
    Animation animation;
    public bool IsRun { get;  set; } = false;
    public IfAnimation Ani = new IfAnimation();

    private void Start()
    {
        animation = Animation.Instance;
        gamePlayManager = GamePlayManager.Instance;
       // GetComponent<Animation>().AniStartButton(uiManager.getStartButton());
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
            StartCoroutine(DelaySort());
            IsRun = true;
        }
        else if (Ani.BlockStart == null && !IsRun && !ScorePlus)
        {
            HandleScore();
        }

    }
    IEnumerator DelaySort()
    {
        yield return new WaitForSeconds(0.1f);
        HandleAnimations();
    }
    private void HandleAnimations()
    {
        var firstAnimation = Ani;

        if (firstAnimation != null && firstAnimation.BlockStart != null && firstAnimation.BlockStart !=null && firstAnimation.BlockEnd != null)
        {
            if (Ani == null)
            {
                Debug.LogWarning("Ani.IfCheck = null, không thể chạy animation");
                return;
            }

            var aniComp = GetComponent<Animation>();
            if (aniComp == null)
            {
                Debug.LogError("Không tìm thấy component Animation trên GameObject");
                return;
            }
            Debug.Log("start");
            aniComp.RunUpBlocks(Ani.BlockEnd, Ani.BlockStart);
        }
        else
        {
            Debug.LogWarning("HandleAnimations bị gọi nhưng dữ liệu Ani chưa hợp lệ");
        }
    }

    private void HandleScore()
    {
        if (gamePlayManager.BottomBlock == null || gamePlayManager.BottomBlock.Count == 0) return;
        Debug.Log(0);
        foreach (var block in gamePlayManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gamePlayManager.MunberBlock || gamePlayManager.CheckScore(block) < gamePlayManager.MunberBlock)
                continue;

            int score = gamePlayManager.CheckScore(block);
            if (score > 0)
            {
                Debug.Log(1);
                gamePlayManager.ScorePluss += score;
                StartCoroutine(animation.PlusScore(block, score, 0));
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
       
        IfAnimation newAnimation = new IfAnimation
        {
            BlockStart = Start,
            BlockEnd = End,
        };
        Ani = newAnimation;
    }
    public void EndAnimation()
    {
        Ani = new IfAnimation();
    }
    public void ChangeInDataBlockControl()
    {
        // check gamePlayManager
        if (gamePlayManager == null || gamePlayManager.BottomBlock == null)
        {
            Debug.LogError("gamePlayManager hoặc BottomBlock đang null!");
            return;
        }

        // check Ani
        if (Ani == null || Ani.BlockStart == null)
        {
            return;
        }

        foreach (var i in gamePlayManager.BottomBlock)
        {
            if (Ani.BlockStart || i == Ani.BlockEnd)
            {
                i.ListChildBlock.Clear();

                foreach (Transform k in i.transform)
                {
                    var child = k.GetComponent<ChildBlock>();
                    if (child != null)
                        i.ListChildBlock.Add(child);
                }
            }
        }
    }

}
