using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Animation : Singleton<Animation>
{
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.05f;

    private AudioControl audioControl;
    private AnimationControl control;
    private GameManager gameManager;
    private UIManager uiManager;

    private void Start()
    {
        audioControl = AudioControl.Instance;
        control = AnimationControl.Instance;
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
    }

    public void ChangeBlock(BlockControl start, BlockControl end, int countBlock)
    {
        List<Transform> listBlockChange = new List<Transform>();
        for (int i = 0; i < countBlock; i++)
        {
            if (start.transform.GetChild(i) == null) continue;
            listBlockChange.Add(start.transform.GetChild(i));
        }

        UpBlock(listBlockChange, end.transform.childCount * gameManager.sizeYBlock + 0.006f, 0.005f, end);
    }
    
    public void StartAni()
    {
        foreach (var block in gameManager.ListBlockGamePlay)
        {
            for (int j = 0; j < block.transform.childCount; j++)
            {
                Vector3 pos = block.transform.GetChild(j).transform.position;
                StartCoroutine(MoveBlocksSequential(block.transform.GetChild(j).transform, 0.05f * j, pos));
            }
        }
    }

    private IEnumerator MoveBlocksSequential(Transform tf, float time, Vector3 pos)
    {
        yield return new WaitForSeconds(time);
        tf.DOMove(new Vector3(pos.x, pos.y - 4, pos.z), 0.1f);
    }

    public void UpBlock(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        StartCoroutine(MoveBlocksSequential(tf, heightUp, distanceBlock, blockEnd));
    }

    private IEnumerator MoveBlocksSequential(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        for (int i = 0; i < tf.Count; i++)
        {
            Vector3 pos = tf[i].localPosition;
            audioControl.StartUp();

            bool done = false;

            tf[i].DOLocalMove(new Vector3(pos.x, heightUp + (tf.Count - i) * distanceBlock, pos.z), TimeUpBlock)
                .OnComplete(() => done = true);

            yield return new WaitUntil(() => done);
        }

        StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.05f));
    }

    IEnumerator WaitMove(List<Transform> tf, BlockControl blockEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        float heightLast = blockEnd.transform.childCount * gameManager.sizeYBlock;
       

        yield return StartCoroutine(MoveChildBlock(tf, blockEnd, 0.05f, heightLast));
    }

    IEnumerator MoveChildBlock(List<Transform> tf, BlockControl blockEnd, float timeWait, float heightLast)
    {
        for (int i = tf.Count - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(timeWait);

            if (tf[i] == null) continue;

            tf[i].SetParent(blockEnd.transform);
            tf[i].transform.SetSiblingIndex(0);

            float newY = tf[i].transform.localPosition.y;
            audioControl.StartMove();
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeMoveBlock);
        }

        yield return new WaitForSeconds(TimeMoveBlock + 0.05f);
        yield return StartCoroutine(WaitDownSequential(tf, blockEnd, heightLast, TimeDownBlock, 0.1f));
        control.IsRun = false;
    }

    IEnumerator WaitDownSequential(List<Transform> tf, BlockControl blockEnd, float lastHeightEnd, float timeWait, float delayBetweenBlocks)
    {
        yield return new WaitForSeconds(timeWait);

        for (int i = tf.Count - 1; i >= 0; i--)
        {
            float newY = lastHeightEnd + ((tf.Count - 1 - i) * gameManager.sizeYBlock);
            audioControl.StartDown();

            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock);

            yield return new WaitForSeconds(delayBetweenBlocks);
        }
        yield return new WaitForSeconds(TimeDownBlock + 0.05f);

        

        HandleScore(blockEnd);
    }

    private void HandleScore(BlockControl blockEnd)
    {
         
         if(control.ListAni.Count > 0 )
            control.ListAni.RemoveAt(0);
        
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
         gameManager.SortAll();
    }

    public void AniStartButton(RectTransform transform)
    {
        transform.DOScale(1.2f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo) 
            .SetEase(Ease.InOutSine);  
    }

  
    public IEnumerator PlusScore(BlockControl block, int score, float delay)
    {
        gameManager.StartScaleScore = false;
        block.ListChildBlock.RemoveRange(0, score);
        yield return new WaitForSeconds(delay);

        List<Transform> children = new List<Transform>();

        for (int i = 0; i < score; i++)
        {
            Transform child = block.transform.GetChild(i);
            if (child != null) children.Add(child);
        }

        float totalDuration = 1f; // Tổng thời gian cho toàn bộ animation
        float singleTweenDuration = totalDuration / children.Count; // Thời gian cho mỗi tween

        foreach (var child in children)
        {
            // Tạo tween với thời gian mỗi tween là singleTweenDuration
            child.DOScale(Vector3.zero, singleTweenDuration);
            yield return null; // Không cần wait, bởi vì DOTween sẽ tự xử lý thời gian
        }

        foreach (var child in children)
        {
          
            child.DOKill();
            if (child != null) Destroy(child.gameObject);
        }
        uiManager.SetActiveScale(false);

       
        RectTransform TransformText = uiManager.GetTransformTextSale();
        TextMeshProUGUI textSale = uiManager.GetTextSale();
        Vector2 startPos = TransformText.anchoredPosition;
        float startFontSize = textSale.fontSize;

        TransformText.DOAnchorPos(new Vector2(400, 520), 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                uiManager.SetTextScale("+" + (gameManager.ScorePluss * ((gameManager.CountScaleScore / 5) + 1)).ToString());
                DOTween.To(() => textSale.fontSize,
                    x => textSale.fontSize = x,
                    30, 0.2f)
                    .OnComplete(() =>
                    {
                        TransformText.DOAnchorPos(new Vector2(400, 600), 0.2f)
                            .SetEase(Ease.OutBack)
                            .OnComplete(() =>
                            {
                              
                                TransformText.DOKill();
                                DOTween.Kill(textSale);

                                uiManager.SetActiveTextScale(false);

                               
                                TransformText.anchoredPosition = startPos;
                                textSale.fontSize = startFontSize;

                                gameManager.UpdateScore();
                            });
                    });
            });
        gameManager.CheckBlock();
        gameManager.SortAll();
        control.ScorePlus = false;
        control.IsRun = false;
    }
}
