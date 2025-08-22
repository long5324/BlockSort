using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControl : Singleton<AnimationControl>
{
    [System.Serializable]
    public class IfAnimation
    {
        public BlockControl BlockStart;
        public BlockControl BlockEnd;
        public int CountBlock;
    }
    private float lastTweenTime = -1f;
    public bool ScorePlus { get;  set; } = false;  
    private Animation animation;
    private GameManager gameManager;
    private UIManager uiManager;
    bool delaysort = false;
    public bool IsRun { get;  set; } = false;  
    public List<IfAnimation> ListAni { get; private set; } = new List<IfAnimation>();

    private void Start()
    {
        gameManager = GameManager.Instance;
        animation = Animation.Instance;
        uiManager = UIManager.Instance;
        animation.AniStartButton(uiManager.getStartButton());
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
            animation.ChangeBlock(firstAnimation.BlockStart, firstAnimation.BlockEnd, firstAnimation.CountBlock);
            IsRun = true;
        }
    }

    private void HandleScore()
    {
        // Đảm bảo BottomBlock không phải là null và có ít nhất một phần tử
        if (gameManager.BottomBlock == null || gameManager.BottomBlock.Count == 0) return;

        foreach (var block in gameManager.BottomBlock)
        {
            if (block.ListChildBlock.Count < gameManager.MunberBlock || gameManager.CheckScore(block) < gameManager.MunberBlock)
                continue;

            int score = gameManager.CheckScore(block);
            if (score > 0)
            {
                gameManager.ScorePluss += score;
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
}
