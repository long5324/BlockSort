using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static AnimationControl;

public class AnimationControl : Singleton<AnimationControl>
{
    [System.Serializable]
    public class IfAnimation
    {
        public BlockControl BlockStart;
        public BlockControl BlockEnd;
        public int CountBlock;
    }
    public bool ScorePlus { get;  set; } = false;  
    private GamePlayManager gamePlayManager;
    private UIManager uiManager;
    bool delaysort = false;
    Animation animation;
    public bool IsRun { get;  set; } = false;  
    public List<IfAnimation> ListAni { get; private set; } = new List<IfAnimation>();

    private void Start()
    {
        animation = Animation.Instance;
        gamePlayManager = GamePlayManager.Instance;
        uiManager = UIManager.Instance;
        GetComponent<Animation>().AniStartButton(uiManager.getStartButton());
    }
    private void Update()
    {
        if (ScorePlus)
        {
            delaysort = true;
        }
        else delaysort = false; 
        if (ListAni.Count > 0 && !IsRun && !ScorePlus && !delaysort)
        {
           StartCoroutine(DelaySort());
            IsRun = true;
        }
        else if (ListAni.Count == 0 && !IsRun && !ScorePlus)
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
        var firstAnimation = ListAni[0];

        if (firstAnimation != null && firstAnimation.BlockStart != null && firstAnimation.BlockEnd != null)
        {
            GetComponent<Animation>().ChangeBlock(firstAnimation.BlockStart, firstAnimation.BlockEnd, firstAnimation.CountBlock);
        }
    }
    private void HandleScore()
    {
        if (gamePlayManager.BottomBlock == null || gamePlayManager.BottomBlock.Count == 0) return;

        foreach (var block in gamePlayManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gamePlayManager.MunberBlock || gamePlayManager.CheckScore(block) < gamePlayManager.MunberBlock)
                continue;

            int score = gamePlayManager.CheckScore(block);
            if (score > 0)
            {
                gamePlayManager.ScorePluss += score;
                StartCoroutine(animation.PlusScore(block, score, 0));
                ScorePlus = true;
            }

        }
    }
    public void AddAni(BlockControl Start, BlockControl End, int countBlock)
    {
        if (Start == null || End == null)
        {
            return;
        }

        IfAnimation newAnimation = new IfAnimation
        {
            BlockStart = Start,
            BlockEnd = End,
            CountBlock = countBlock
        };

        ListAni.Add(newAnimation);
    }
    public void EndAnimation()
    {
        ListAni.RemoveAt(0);
    }
    public void ChangeInDataBlockControl()
    {
       foreach(var i in gamePlayManager.BottomBlock)
        {
            if(i == ListAni[0].BlockStart)
            {
                i.ListChildBlock.Clear();
                foreach(Transform j in i.transform)
                {
                    i.ListChildBlock.Add(j.GetComponent<ChildBlock>());
                }
            }
            else if(i == ListAni[0].BlockEnd)
            {
                i.ListChildBlock.Clear();
                foreach (Transform j in i.transform)
                {
                    i.ListChildBlock.Add(j.GetComponent<ChildBlock>());
                }
            }
        }
    }
}
