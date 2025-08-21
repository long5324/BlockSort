using DG.Tweening.Core.Easing;
using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq.Expressions;
using UnityEngine.UIElements;

public class AnimationControl : Singleton<AnimationControl>
{
    [System.Serializable]
    public class IfAnimation
    {
        public BlockControl BlockStart;
        public BlockControl BlockEnd;
        public int CountBlock;
    }
    public bool ScorePlus { get; set; } = false;
    Animation animation;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
        animation = Animation.Instance;
    }
    public bool IsRun { get; set; } = false;
    public List<IfAnimation> ListAni = new List<IfAnimation>();
    private void LateUpdate()
    {
        if (ListAni.Count > 0 && !IsRun && !ScorePlus) {
            animation.ChangeBlock(ListAni[0].BlockStart, ListAni[0].BlockEnd, ListAni[0].CountBlock);
            IsRun = true;
        }
        if (ListAni.Count == 0 && !IsRun && !ScorePlus) {
            foreach (var i in gameManager.BottomBlock)
            {
                if (i.ListChildBlock.Count<gameManager.MunberBlock||gameManager.CheckScore(i) < gameManager.MunberBlock) continue;
                Debug.Log(gameManager.CheckScore(i));
                    gameManager.ScorePluss += gameManager.CheckScore(i);
                    StartCoroutine(animation.PlusScore(i, gameManager.CheckScore(i), 0));
                    ScorePlus = true;  
            }
        }
    }
    public void AddAni(BlockControl Start , BlockControl End , int countBlock)
    {
        IfAnimation N = new IfAnimation();
        N .BlockStart = Start;
        N .BlockEnd = End; 
        N .CountBlock = countBlock;
        ListAni.Add(N);
    }
}
