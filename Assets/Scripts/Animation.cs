using DG.Tweening;
using DG.Tweening.Core.Easing;
using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Animation : Singleton<Animation>
{
    public EffectData particleObject;
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.05f;
    DataInport Data;
    private void Start()
    {
        Data = DataInport.Ins;
    }

    public void RunUpBlocks(BlockControl start, BlockControl end)
    {
        // Lấy danh sách child block từ block start
        List<Transform> tf = new List<Transform>();
        List<ChildBlock> lbc = start.GetSameBlock();

        foreach (var child in lbc)
        {
            tf.Add(child.transform);
        }

        // Chạy coroutine UpBlock (không cần chờ)
        StartCoroutine(UpBlock(
            tf,
            end.transform.childCount * Data.gamePlayManager.sizeYBlock + 0.006f,
            0.005f,
            end
        ));
    }


    public IEnumerator UpBlock(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        yield return StartCoroutine(MoveBlocksSequential(tf, heightUp, distanceBlock, blockEnd));
    }


    private IEnumerator MoveBlocksSequential(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        for (int i = 0; i < tf.Count; i++)
        {
            Vector3 pos = tf[i].localPosition;
            AudioControl.Ins.PlaySFX(AudioControl.up);


            bool done = false;

            tf[i].DOLocalMove(new Vector3(pos.x, heightUp + (tf.Count - i) * distanceBlock, pos.z), TimeUpBlock)
                .OnComplete(() => done = true);

            yield return new WaitUntil(() => done);
        }
        yield return StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.02f));
    }

    IEnumerator WaitMove(List<Transform> tf, BlockControl blockEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        float heightLast = blockEnd.transform.childCount * Data.gamePlayManager.sizeYBlock;
       

        yield return StartCoroutine(MoveChildBlock(tf, blockEnd, 0.02f, heightLast));
    }

    IEnumerator MoveChildBlock(List<Transform> tf, BlockControl blockEnd, float timeWait, float heightLast)
    {
        for (int i = tf.Count - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(timeWait);
            if (tf[i] == null) continue;
            tf[i].SetParent(blockEnd.transform);
            float newY = tf[i].transform.localPosition.y;
            AudioControl.Ins.PlaySFX(AudioControl.move);


            bool done = false;
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeMoveBlock)
                 .OnComplete(() => done = true);

            yield return new WaitUntil(() => done); 
        }

        yield return new WaitForSeconds(TimeMoveBlock + 0.01f);
        yield return StartCoroutine(WaitDownSequential(tf, blockEnd, heightLast, TimeDownBlock, 0.05f));
    }


    IEnumerator WaitDownSequential(List<Transform> tf, BlockControl blockEnd, float lastHeightEnd, float timeWait, float delayBetweenBlocks)
    {
        yield return new WaitForSeconds(timeWait);
        
        for (int i = tf.Count - 1; i >= 0; i--)
            {
                if (tf[i] == null) continue;

                float newY = lastHeightEnd + ((tf.Count - i) * Data.gamePlayManager.sizeYBlock);
               AudioControl.Ins.PlaySFX(AudioControl.down);

                bool done = false;
                tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock)
                     .OnComplete(() => done = true);

                yield return new WaitUntil(() => done);
                yield return new WaitForSeconds(delayBetweenBlocks);
            }

            yield return new WaitForSeconds(TimeDownBlock);

       
        Vector3 BlockStart = Data.animationControl.Ani.BlockStart.PosionBlock;
        Vector3 BlockEnd = Data.animationControl.Ani.BlockEnd.PosionBlock;
        Data.animationControl.IsRun = false;
        Data.animationControl.ChangeInDataBlockControl(BlockStart);
        Data.animationControl.ChangeInDataBlockControl(BlockEnd);
        AddCheck(BlockStart);
        AddCheck(BlockEnd);
    }

    void AddCheck (Vector3 BlockList)
    {
        Data.gamePlayManager.DelayCheck.Add(BlockList);
    }
    public void AniStartButton(RectTransform transform)
    {
        transform.DOScale(1.2f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo) 
            .SetEase(Ease.InOutSine);  
    }
    public IEnumerator PlusScore(BlockControl block, int score, float delay, bool c)
    {
        ParticleSystem Particle = particleObject.StartEffect(block.CheckColor(), block.PosionBlock);
        Data.gamePlayManager.StartScaleScore = false;
        yield return new WaitForSeconds(delay);
        List<Transform> children = new List<Transform>();
        Debug.Log(block.PosionBlock);
        int takeCount = Mathf.Min(score, block.transform.childCount);
        List<BlockControl> BlockLockCount = new List<BlockControl>();
        List<BlockControl> BlockArow = block.BlockLink;
        foreach(var i in BlockArow)
        {
            if (i.State == StateBlock.LockCount)
            {
                BlockLockCount.Add(i);
            }
        }
        for (int i = block.transform.childCount - 1; i >= block.transform.childCount - takeCount; i--)
        {
            Transform child = block.transform.GetChild(i);
            children.Add(child);
        }
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] != null)
            {
                Particle.transform.position = children[i].position;
                yield return children[i].DOScale(Vector3.zero, 0.06f).WaitForCompletion();
                for (int k = BlockLockCount.Count - 1; k >= 0; k--)
                {
                    var j = BlockLockCount[k];
                    j.NumberLockCount--;
                    j.TextLockCount.text = j.NumberLockCount.ToString();

                    if (j.NumberLockCount == 0)
                    {
                        GamePlayManager.Ins.DelayCheck.Add(BlockLockCount[k].PosionBlock);
                        BlockLockCount[k].BackNomal();
                        BlockLockCount.RemoveAt(k);

                    }
                }


                LeanPool.Despawn(children[i]);
            }

        }
        Destroy(Particle);
        Data.gamePlayManager.CurrenScore += children.Count;
        UIManager.Ins.GetUI<GameplayUI>().SetFillScore(Data.gamePlayManager.CurrenScore, Data.gameManager.MaxCurrenScore);
        UIManager.Ins.GetUI<GameplayUI>().SetTextScore(Data.gamePlayManager.CurrenScore.ToString() + "/" + Data.gameManager.MaxCurrenScore.ToString());
        if (Data.gamePlayManager.CurrenScore >= Data.gameManager.MaxCurrenScore)
        {
            Data.gameManager.Winlevel();
        }
        Data.animationControl.ChangeInDataBlockControl(block.PosionBlock);
        if (Data.gamePlayManager.CurrenScore > Data.gameManager.MaxCurrenScore)
        {
            //  Data.gameManager.Winlevel();
        }
        AddCheck(block.PosionBlock);
        if (c)
        {
            Data.animationControl.ScorePlus = false;
            Debug.Log("ok check");
        }
    }
    }
