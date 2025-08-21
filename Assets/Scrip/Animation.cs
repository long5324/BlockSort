using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using NUnit;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;

public class Animation : Singleton<Animation>
{
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.1f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.1f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.1f;
    AudioControl audioControl;
    AnimationControl control;
    GameManager gameManager;
    UIManager uiManager;

    private void Start()
    {
        audioControl = AudioControl.Instance;
        control = AnimationControl.Instance;    
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
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
        StartCoroutine(MoveBlocksSequential(tf, hightUp, distanceBlock, blockEnd));
    }

    private IEnumerator MoveBlocksSequential(List<Transform> tf, float hightUp, float distanceBlock, BlockControl blockEnd)
    {
        for (int i = 0; i < tf.Count; i++)
        {
            Vector3 pos = tf[i].localPosition;
            audioControl.StartUp();

            bool done = false;

            tf[i].DOLocalMove(
                new Vector3(pos.x, hightUp + (tf.Count - i) * distanceBlock, pos.z),
                TimeUpBlock
            ).OnComplete(() => done = true);

            // chờ tween hiện tại kết thúc
            yield return new WaitUntil(() => done);
        }

        // sau khi tất cả xong, gọi WaitMove
        StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.1f));
    }


    IEnumerator WaitMove(List<Transform> tf, BlockControl blockEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        float hightLast = blockEnd.transform.childCount * gameManager.sizeYBlock;
        audioControl.StartMove();
        yield return StartCoroutine(MoveChildBlock(tf, blockEnd, 0.1f, hightLast));
    }

    IEnumerator MoveChildBlock(List<Transform> tf, BlockControl blockEnd, float timeWait, float hightLast)
    {
        for (int i = tf.Count-1; i >=0; i--)
        {
            yield return new WaitForSeconds(timeWait);
            if (tf[i] == null) continue;
            tf[i].SetParent(blockEnd.transform);
            tf[i].transform.SetSiblingIndex(0);
            float newY = tf[i].transform.localPosition.y;
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeMoveBlock);
        }
        yield return new WaitForSeconds(TimeMoveBlock + 0.05f);
        yield return StartCoroutine(WaitDownSequential(tf, blockEnd, hightLast, TimeDownBlock,0.1f));
    }
    IEnumerator WaitDownSequential(List<Transform> tf, BlockControl blockEnd, float lastHightEnd, float timeWait, float delayBetweenBlocks)
    {
        // chờ ban đầu
        yield return new WaitForSeconds(timeWait);

        for (int i = tf.Count - 1; i >= 0; i--)
        {
            float newY = lastHightEnd + ((tf.Count - 1 - i) * gameManager.sizeYBlock);
            audioControl.StartDown();

            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock);

            // đợi 1 khoảng trước khi hạ block tiếp theo
            yield return new WaitForSeconds(delayBetweenBlocks);
        }

        // sau khi tất cả hạ xong
        control.IsRun = false;
        control.ListAni.RemoveAt(0);

        // xử lý scale score
        if (gameManager.CheckScore(blockEnd) >= gameManager.MunberBlock && !gameManager.StartScaleScore)
        {
            gameManager.StartScaleScore = true;
            uiManager.SetActiveScale(true);
            uiManager.SetActiveTextScale(true);
        }

        if (gameManager.StartScaleScore)
        {
            gameManager.CountScaleScore++;
            float value = (gameManager.CountScaleScore % 5) / 5f;
            uiManager.SetScoreValue(value);
            uiManager.SetTextScale("x" + (gameManager.CountScaleScore / 5 + 1).ToString());
        }
    }

    public IEnumerator PlusScore(BlockControl block, int score, float delay)
    {
        gameManager.StartScaleScore = false;
        block.ListChildBlock.RemoveRange(0, score);
        yield return new WaitForSeconds(delay);
        List<Transform> children = new List<Transform>();
        int count = Mathf.Min(score, block.transform.childCount);

        for (int i = 0; i < count; i++)
        {
            Transform child = block.transform.GetChild(i);
            if (child != null) children.Add(child);
        }

        foreach (var child in children)
        {
            child.DOScale(Vector3.zero, 0.5f);
            yield return new WaitForSeconds(0.05f);
        }
        foreach (var child in children)
        {
            child.DOKill();
            if (child != null) Destroy(child.gameObject);
        }
        
        control.ScorePlus = false;
        gameManager.SortAll();
        uiManager.SetActiveScale(false);
        RectTransform TransformText = uiManager.GetTransformTextSale();
        TextMeshProUGUI textSale = uiManager.GetTextSale();

        // Lưu trạng thái ban đầu
        Vector2 startPos = TransformText.anchoredPosition;
        float startFontSize = textSale.fontSize;

        TransformText.DOAnchorPos(new Vector2(400, 520), 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            uiManager.SetTextScale("+" + (gameManager.ScorePluss * ((gameManager.CountScaleScore / 5) + 1)).ToString());
            DOTween.To(() => textSale.fontSize,
                       x => textSale.fontSize = x,
                       30, 0.2f).OnComplete(() =>
                       {
                           TransformText.DOAnchorPos(new Vector2(400, 600), 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
                           {
                               // Kill tween trước khi reset
                               TransformText.DOKill();
                               DOTween.Kill(textSale);

                               uiManager.SetActiveTextScale(false);

                               // Reset lại trạng thái ban đầu
                               TransformText.anchoredPosition = startPos;
                               textSale.fontSize = startFontSize;

                               gameManager.UpdateScore();
                           });
                       });
        });




    }



}
