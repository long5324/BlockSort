using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Metadata;

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
    Vector3 SalceCahe;
    List<BlockControl> CheckList = new List<BlockControl>();
    public IEnumerator PlusScore(BlockControl block, int score, float delay, bool c)
    {
        ParticleSystem Particle = particleObject.StartEffect(block.CheckColor(), block.PosionBlock);
        Data.gamePlayManager.StartScaleScore = false;
        yield return new WaitForSeconds(delay);
        List<Transform> children = new List<Transform>();

        int takeCount = Mathf.Min(score, block.transform.childCount);
        List<BlockControl> BlockArow = block.BlockLink;
        for (int i = block.transform.childCount - 1; i >= block.transform.childCount - takeCount; i--)
        {
            Transform child = block.transform.GetChild(i);
            
            children.Add(child);
        }
        BlockControl bc = null;
        foreach (var j in BlockArow)
        {
            if (j.State == StateBlock.Support)
            {
                SalceCahe = j.transform.localScale;
                bc = j; break;
            }
        }
        if (bc != null) Squash(bc, 0.06f * children.Count);
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                {
                    Particle.transform.position = children[i].position;
                    yield return children[i].DOScale(Vector3.zero, 0.06f).WaitForCompletion();
                    LeanPool.Despawn(children[i]);
                    block.ListChildBlock[block.ListChildBlock.Count - 1 - i].SetDefaultBlockChild();
                }
            }
            GamePlayManager.Ins.UpdateSocre(children.Count);
            Destroy(Particle);
            Data.animationControl.ChangeInDataBlockControl(block.PosionBlock);
            AddCheck(block.PosionBlock);
            CheckList.Add(block);
            if (c)
            {
                Data.animationControl.ScorePlus = false;
            }
            foreach (var i in BlockArow)
            {
                if (i.State == StateBlock.LockCount)
                {
                    GameManager.Ins.CurrenGridLevel.SpawnEffect(i);
                    i.DeleteLockCount();
                    GamePlayManager.Ins.ShakeObject(i.transform);
                    if (i.CheckCount() == 1)
                    {
                        GamePlayManager.Ins.ShakeObject(i.transform);
                        i.BackNomal();
                        i.SetColor(GamePlayManager.Ins.MaterialDF);
                        Destroy(i.transform.GetChild(0));
                    }
                }
                else if (i.State == StateBlock.Support)
                {
                    Stretch(i);

                }
            
        }
    }
    // Hàm nén
    public Tween Squash(BlockControl targetTransform , float time)
    {
        Vector3 originalScale = targetTransform.transform.localScale;

        // Scale nén xuống (chỉ bé trục Y, phình trục X/Z cho cảm giác mềm mại)
        Vector3 squashScale = new Vector3(
            originalScale.x * 1.2f,
            originalScale.y * 0.5f,
            originalScale.z * 1.2f
        );

        return targetTransform.transform
            .DOScale(squashScale, time)
            .SetEase(Ease.InQuad);
    }

    // Hàm bật ra
    public Tween Stretch(BlockControl targetTransform)
    {
        return targetTransform.transform
            .DOScale(SalceCahe, 0.2f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                // Chạy event sau khi bật ra
                BlockControl Bc = ChooseRandomBlock();
                GamePlayManager.Ins.EventSupport(targetTransform, Bc);
            });
    }



    public BlockControl ChooseRandomBlock()
    {
        List<BlockControl> AllBlock = GamePlayManager.Ins.BottomBlock;
        List<BlockControl> ListBlockRandom = new List<BlockControl>();
        foreach (var i in AllBlock)
        {
            if(i.ListChildBlock.Count > 0)
            {
                  ListBlockRandom.Add(i);
            }
        }
        int R = Random.Range(0,ListBlockRandom.Count);
        return ListBlockRandom[R];
    }
    }
