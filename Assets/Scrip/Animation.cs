using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using NUnit;
using UnityEngine.SocialPlatforms.Impl;

public class Animation : Singleton<Animation>
{
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.3f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.2f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.2f;
    AnimationControl control;
    GameManager gameManager;

    private void Start()
    {
        control = AnimationControl.Instance;    
        gameManager = GameManager.Instance;
    }

    public void ChangeBlock(BlockControl Start, BlockControl End, int countBlock)
    {
        List<Transform> listBlockChange = new List<Transform>();
        for (int i = 0; i < countBlock; i++)
        {
            listBlockChange.Add(Start.transform.GetChild(i));
        }

            Upblock(listBlockChange, End.transform.childCount * gameManager.sizeYBlock + 0.006f, 0.005f, End);
        

    }

    public void Upblock(List<Transform> tf, float hightUp, float distanceBlock, BlockControl blockEnd)
    {
        for (int i = 0; i < tf.Count; i++)
        {
            Vector3 pos = tf[i].localPosition;
            tf[i].DOLocalMove(
                new Vector3(pos.x, hightUp + (tf.Count - i) * distanceBlock, pos.z),
                TimeUpBlock
            );
        }

        StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.1f));
    }

    IEnumerator WaitMove(List<Transform> tf, BlockControl blockEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        float hightLast = blockEnd.transform.childCount * gameManager.sizeYBlock;
        yield return StartCoroutine(MoveChildBlock(tf, blockEnd, 0.1f, hightLast));
    }

    IEnumerator MoveChildBlock(List<Transform> tf, BlockControl blockEnd, float timeWait, float hightLast)
    {
        for (int i = 0; i < tf.Count; i++)
        {
            yield return new WaitForSeconds(timeWait);

            tf[i].SetParent(blockEnd.transform);
            tf[i].transform.SetSiblingIndex(0);
            float newY = tf[i].transform.localPosition.y;
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeMoveBlock);
        }
        yield return new WaitForSeconds(TimeMoveBlock + 0.05f);
        yield return StartCoroutine(WaitDown(tf, blockEnd, hightLast, TimeDownBlock));
    }
    IEnumerator WaitDown(List<Transform> tf, BlockControl blockEnd, float lastHightEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        for (int i = tf.Count - 1; i >= 0; i--)
        {
            float newY = lastHightEnd + ((tf.Count - 1 - i) * gameManager.sizeYBlock);
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock);
        }
        control.IsRun = false;
        control.ListAni.RemoveAt(0);
    }
    public IEnumerator PlusScore(BlockControl block, int score, float delay)
    {
        yield return new WaitForSeconds(delay);
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < score; i++)
        {
            children.Add(block.transform.GetChild(i));
        }
        foreach (var child in children)
        {
            child.DOScale(Vector3.zero, 0.5f);
            yield return new WaitForSeconds(0.07f);
        }
        foreach (var child in children)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }
        Debug.Log(score + " t " + block.ListChildBlock.Count);
        block.ListChildBlock.RemoveRange(0, score);
        control.ScorePlus = false;
        gameManager.SortAll();
    }



}
