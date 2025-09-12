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
public class Animation : Singleton<Animation>
{
  
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

        //Chạy coroutine UpBlock (không cần chờ)
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
    BlockControl BCT = null;
    public IEnumerator PlusScore(BlockControl block, int score, float delay, bool c)
    {
        yield return new WaitForSeconds(delay);

        // 1. Lấy danh sách child sẽ xoá
        var children = GetChildrenToRemove(block, score);

        // 2. Lấy block Support
        BlockControl bc = GetSupportBlock(block);

        // 3. Spawn effect
        ParticleSystem effect = SpawnEatEffect(block, score);

        // 4. Nếu có block Support thì squash
        if (bc != null) Squash(bc, 0.06f * children.Count);

        // 5. Xử lý xoá từng child + animation
        yield return RemoveChildren(block, children, effect);

        // 6. Xoá effect
        DestroyEffect(effect);

        // 7. Update score + data
        FinalizeScore(block, children);

        // 8. Xử lý tiếp (cờ c = true)
        HandleContinue(block, bc, c);

        // 9. Xử lý các block LockCount
        HandleLockBlocks(block);
    }
    private List<Transform> GetChildrenToRemove(BlockControl block, int score)
    {
        List<Transform> children = new List<Transform>();
        int takeCount = Mathf.Min(score, block.transform.childCount);

        for (int i = block.transform.childCount - 1; i >= block.transform.childCount - takeCount; i--)
        {
            children.Add(block.transform.GetChild(i));
        }
        return children;
    }

    private BlockControl GetSupportBlock(BlockControl block)
    {
        foreach (var j in block.BlockLink)
        {
            if (j.State == StateBlock.Support)
            {
                SalceCahe = j.transform.localScale;
                return j;
            }
        }
        return null;
    }

    private ParticleSystem SpawnEatEffect(BlockControl block, int score)
    {
        ParticleSystem effect = Instantiate(GamePlayManager.Ins.EffectBlockEat, block.transform);
        effect.transform.localPosition = new Vector3(0, score * GamePlayManager.Ins.sizeYBlock, 0);
        effect.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        var main = effect.main;
        main.startColor = block.ListChildBlock[^1].MeshRenderer.material.color;
        return effect;
    }

    private IEnumerator RemoveChildren(BlockControl block, List<Transform> children, ParticleSystem effect)
    {
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] != null)
            {
                effect.transform.localPosition = children[i].localPosition;
                yield return children[i].DOScale(Vector3.zero, 0.06f).WaitForCompletion();

                LeanPool.Despawn(children[i]);
                block.ListChildBlock[block.ListChildBlock.Count - 1 - i].SetDefaultBlockChild();
            }
        }
    }

    private void DestroyEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(effect.gameObject);
        }
    }

    private void FinalizeScore(BlockControl block, List<Transform> children)
    {
        GamePlayManager.Ins.UpdateSocre(children.Count);
        Data.animationControl.ChangeInDataBlockControl(block.PosionBlock);
        AddCheck(block.PosionBlock);
    }

    private void HandleContinue(BlockControl block, BlockControl bc, bool c)
    {
        if (c)
        {
            Data.animationControl.ScorePlus = false;
            if (bc != null && bc.State == StateBlock.Support)
            {
                Data.animationControl.IsRun = true;
                Data.animationControl.Ani = null;
                Stretch(bc);
            }
        }
    }

    private void HandleLockBlocks(BlockControl block)
    {
        foreach (var i in block.BlockLink)
        {
            if (i.State == StateBlock.LockCount)
            {
                HandleBlockBlockTarget(i);
            }
        }
    }
    public void HandleBlockBlockTarget(BlockControl BlockLock)
    {
        GameManager.Ins.CurrenGridLevel.SpawnEffect(BlockLock);
        BlockLock.DeleteLockCount();
        GamePlayManager.Ins.ShakeObject(BlockLock.transform);
        if (BlockLock.CheckCount() == 1)
        {
            GamePlayManager.Ins.ShakeObject(BlockLock.transform);
            BlockLock.BackNomal();
            BlockLock.SetColor(GamePlayManager.Ins.MaterialDF);
            Destroy(BlockLock.transform.GetChild(0).gameObject);
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
                BCT = null;
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
