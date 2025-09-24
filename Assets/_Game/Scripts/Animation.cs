using DG.Tweening;
using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using Lean.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static Lean.Pool.LeanGameObjectPool;
using static UnityEngine.GraphicsBuffer;
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
        Vector3 BlockStart = new Vector3();
        Vector3 BlockEnd = new Vector3();
        if (Data.animationControl.Ani.BlockStart != null)
        {
             BlockStart = Data.animationControl.Ani.BlockStart.PosionBlock;
             BlockEnd = Data.animationControl.Ani.BlockEnd.PosionBlock;
        }
        //Debug.Log(Data.animationControl.Ani.BlockStart.PosionBlock + " " + Data.animationControl.Ani.BlockEnd.PosionBlock);
        Data.animationControl.IsRun = false;
        Data.animationControl.ChangeInDataBlockControl(BlockStart);
       
        Data.animationControl.ChangeInDataBlockControl(BlockEnd);
        AnimationControl.Ins.Ani = new IfData();
        AddCheck(BlockStart);
        AddCheck(BlockEnd);
        GameManager.Ins.EventEndGame();
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
    BlockControl bc = null;
    private int MaxScoreSeen = int.MinValue;
    private BlockControl maxBlock;
    private bool handled = false;

    private int totalCalls = 0;

    public IEnumerator PlusScore(BlockControl block, int score, float delay, bool c)
    {
        totalCalls++;

        if (score > MaxScoreSeen)
        {
            MaxScoreSeen = score;
            maxBlock = block;
        }

        yield return new WaitForSeconds(delay);

        var children = GetChildrenToRemove(block, score);
        ParticleSystem effect = SpawnEatEffect(block, score);

        if (bc == null && GetSupportBlock(block) != null)
        {
            bc = GetSupportBlock(block);
            Squash(bc, 0.06f * children.Count);
        }

        yield return RemoveChildren(block, children, effect);
        if (!handled && ((totalCalls == 1) || (score == MaxScoreSeen)))
        {
            handled = true;
            HandleContinue(maxBlock);
        }
        DestroyEffect(effect);
        FinalizeScore(block, children);
        HandleLockBlocks(block);
        GameManager.Ins.EventEndGame();
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

    private IEnumerator RemoveChildren(BlockControl block, List<Transform> children,ParticleSystem effect)
    {
        Color color = block.ListChildBlock[block.ListChildBlock.Count - 1].MeshRenderer.material.color;
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

    private void HandleContinue( BlockControl bc )
    {
       
            handled = false;
            MaxScoreSeen = 0;
            Data.animationControl.ScorePlus = false;
            BlockControl bcc = GetSupportBlock(bc);
            if (bcc != null && bcc.State == StateBlock.Support)
            {
                
                Data.animationControl.IsRun = true;
                Stretch(bcc);
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
        if (BlockLock.CheckCount() == 0)
        {
            GamePlayManager.Ins.ShakeObject(BlockLock.transform);
            BlockLock.BackNomal();
            BlockLock.SetColor(GamePlayManager.Ins.MaterialDF);
            Destroy(BlockLock.transform.GetChild(0).gameObject);
        }
    }
    // Hàm nén
    public Tween Squash(BlockControl targetTransform, float time)
    {
        Vector3 originalScale = targetTransform.transform.localScale;
        Vector3 squashScale = new Vector3(
            originalScale.x * 1.2f,
            originalScale.y * 0.5f,
            originalScale.z * 1.2f
        );

        return targetTransform.transform
            .DOScale(squashScale, time)
            .SetEase(Ease.InQuad);
    }

    public Tween Stretch(BlockControl targetTransform)
    {
        targetTransform.transform.DOKill();

        BlockControl Bc = ChooseRandomBlock();
        GamePlayManager.Ins.EventSupport(targetTransform, Bc);
        bc = null;
        Vector3 targetScale = new Vector3(43f, 43f, 43f);
        Vector3 stretchScale = targetScale * 1.2f; // phình ra 20% so với (43,43,43)
        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(targetTransform.transform
            .DOScale(stretchScale, 0.1f)
            .SetEase(Ease.OutQuad)); // phình ra
        seq.Append(targetTransform.transform
            .DOScale(targetScale, 0.15f)
            .SetEase(Ease.OutBounce)); // quay về (43,43,43)
        return seq;
    }
    public void SetUpNewPositon()
    {
        foreach (var block in GamePlayManager.Ins.BottomBlock)
        {
            if (block.transform.childCount == 0) continue;
            List<Vector3> oldPositions = new List<Vector3>();
            List<Transform> children = new List<Transform>();

            foreach (Transform child in block.transform)
            {
                oldPositions.Add(child.localPosition);
                children.Add(child);
                child.localPosition = new Vector3(0, 0.1f, 0);
                child.gameObject.SetActive(true);
            }
        }
    }
    public void EffectBlockChildTransition(System.Action onComplete = null)
    {
        int activeBlocks = 0; // Đếm số block có child

        foreach (var block in GamePlayManager.Ins.BottomBlock)
        {
            if (block.transform.childCount == 0) continue;

            activeBlocks++;

            List<Vector3> oldPositions = new List<Vector3>();
            List<Transform> children = new List<Transform>();

            foreach (Transform child in block.transform)
            {
                oldPositions.Add(child.localPosition);
                children.Add(child);

                child.localPosition = new Vector3(0, 0.1f, 0);
                child.gameObject.SetActive(true);
            }

            // Mỗi block sẽ báo hoàn thành 1 lần
            StartCoroutine(MoveChildrenBack(children, oldPositions, () =>
            {
                activeBlocks--;

                // Chỉ khi tất cả block xong thì mới gọi callback
                if (activeBlocks == 0)
                    onComplete?.Invoke();
            }));
        }

        // Nếu không có block nào có child thì gọi luôn callback
        if (activeBlocks == 0)
            onComplete?.Invoke();
    }

    private IEnumerator MoveChildrenBack(List<Transform> children, List<Vector3> oldPositions, System.Action onComplete)
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].DOLocalMove(oldPositions[i], 0.3f);
            yield return new WaitForSeconds(0.1f);
        }

        // Báo cho block này đã xong
        onComplete?.Invoke();
    }
    public BlockControl ChooseRandomBlock()
    {
        List<BlockControl> allBlock = GamePlayManager.Ins.BottomBlock;
        List<BlockControl> listBlockRandom = new List<BlockControl>();

        foreach (var block in allBlock)
        {
            if (block.ListChildBlock.Count > 0 && block.State == StateBlock.Nomal && block.ListChildBlock[block.ListChildBlock.Count-1].CurrenColor != BlockColor.None)
            {
                listBlockRandom.Add(block);
            }
        }

        if (listBlockRandom.Count == 0)
        {
          
            return null;
        }

        int r = UnityEngine.Random.Range(0, listBlockRandom.Count);
        return listBlockRandom[r];
    }
    public void SetupTransformLevelGrid(System.Action onComplete = null)
    {
        Transform GameLevelTransForrm = GameManager.Ins.CurrenLevelGameObject.transform;
        GameLevelTransForrm.localPosition = new Vector3(5, 0, -5);
        GameLevelTransForrm.localScale = new Vector3(1.5f, 0.6f, 1.2f);

        DG.Tweening.Sequence seq = DOTween.Sequence();

        // 1. Move lên
        seq.Append(GameLevelTransForrm.DOLocalMove(Vector3.zero , 0.7f).SetEase(Ease.OutBack));
        // 2. Scale về (chạy sau khi move xong)
        seq.Join(GameLevelTransForrm.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        // 3. Callback khi xong toàn bộ
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    public void AnimationVictoryGameLevel()
    {
        Transform target = GameManager.Ins.LevelGame.transform.GetChild(0);
        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Join(target.DORotate(new Vector3(0, 180f, 0), 3f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        seq.Join(target.DOScale(Vector3.zero, 3f).SetEase(Ease.Linear));

    }

    #region Move Random Child Block
    /*  public void SetupTransformLevelGrid(Action onComplete = null)
      {
          // Đặt tất cả xuống y = -10 trước
          foreach (var i in GamePlayManager.Ins.BottomBlock)
          {
              i.transform.position = new Vector3(i.transform.position.x, -30, i.transform.position.z);
          }
          // Tạo list index
          List<int> IndexRandom = new List<int>();
          for (int i = 0; i < GamePlayManager.Ins.BottomBlock.Count; i++)
          {
              IndexRandom.Add(i);
          }
          // Chạy coroutine để animate từng cái 1
          StartCoroutine(PlayBlocks(IndexRandom, onComplete));
      }

      private IEnumerator PlayBlocks(List<int> IndexRandom, Action onComplete)
      {
          while (IndexRandom.Count > 0)
          {
              int randomValue = GetRandomFromList(IndexRandom);
              IndexRandom.Remove(randomValue);

              Transform block = GamePlayManager.Ins.BottomBlock[randomValue].transform;
              float targetY = GamePlayManager.Ins.BottomBlock[randomValue].PosionBlock.y;
              foreach(Transform i in block)
              {
                  i.gameObject.SetActive(false);
              }
              // Tween block này
              block.DOMoveY(targetY, 0.5f).SetEase(Ease.OutBack);

              yield return new WaitForSeconds(0.05f);
          }

          // Khi chạy xong toàn bộ thì gọi hàm onComplete
          onComplete?.Invoke();
      }

      int GetRandomFromList(List<int> list)
      {
          if (list == null || list.Count == 0) return -1;
          int randIndex = UnityEngine.Random.Range(0, list.Count);
          return list[randIndex];
      }*/
    #endregion
}


