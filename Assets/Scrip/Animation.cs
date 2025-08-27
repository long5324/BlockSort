using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static ObjectBoolingControler;
using static Unity.VisualScripting.Metadata;

public class Animation : Singleton<Animation>
{
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.06f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.06f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.06f;
    private ObjectBoolingControler Booling;
    private AudioControl audioControl;
    private AnimationControl control;
    private GamePlayManager gamePlayManager;
    private UIManager uiManager;

    private void Start()
    {
        Booling = ObjectBoolingControler.Instance;
        audioControl = AudioControl.Instance;
        control = AnimationControl.Instance;
        gamePlayManager = GamePlayManager.Instance;
        uiManager = UIManager.Instance;
    }

    public void ChangeBlock(BlockControl start, BlockControl end, int countBlock)
    {
        List<Transform> listBlockChange = new List<Transform>();

        int startIndex = start.transform.childCount - 1; 
        int endIndex = Mathf.Max(startIndex - countBlock + 1, 0);
        for (int i = startIndex; i >= endIndex; i--)
        {
            Transform child = start.transform.GetChild(i);
            if (child == null || start.ListChildBlock[i].CurrenColor != end.ListChildBlock[end.ListChildBlock.Count-1].CurrenColor) continue; 
            listBlockChange.Add(child);
        }

        UpBlock(listBlockChange, end.transform.childCount * gamePlayManager.sizeYBlock + 0.006f, 0.005f, end);
    }

    public void UpBlock(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        StartCoroutine(MoveBlocksSequential(tf, heightUp, distanceBlock, blockEnd));
    }

    private IEnumerator MoveBlocksSequential(List<Transform> tf, float heightUp, float distanceBlock, BlockControl blockEnd)
    {
        for (int i =0 ; i < tf.Count ; i++)
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

        float heightLast = blockEnd.transform.childCount * gamePlayManager.sizeYBlock;
       

        yield return StartCoroutine(MoveChildBlock(tf, blockEnd, 0.05f, heightLast));
    }

    IEnumerator MoveChildBlock(List<Transform> tf, BlockControl blockEnd, float timeWait, float heightLast)
    {
        for (int i = tf.Count - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(timeWait);

            if (tf[i] == null) continue;

            tf[i].SetParent(blockEnd.transform);

            float newY = tf[i].transform.localPosition.y;
            audioControl.StartMove();
            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeMoveBlock);
        }

        yield return new WaitForSeconds(TimeMoveBlock + 0.05f);
        yield return StartCoroutine(WaitDownSequential(tf, blockEnd, heightLast, TimeDownBlock, 0.1f));
      
    }

    IEnumerator WaitDownSequential(List<Transform> tf, BlockControl blockEnd, float lastHeightEnd, float timeWait, float delayBetweenBlocks)
    {
        yield return new WaitForSeconds(timeWait);

        for (int i = tf.Count - 1; i >= 0; i--)
        {
            float newY = lastHeightEnd + ((tf.Count  - i) * gamePlayManager.sizeYBlock);
            audioControl.StartDown();

            tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock);

            yield return new WaitForSeconds(delayBetweenBlocks);
        }
        yield return new WaitForSeconds(TimeDownBlock + 0.05f);
        control.ChangeInDataBlockControl();
        if (control.ListAni[0].BlockStart.ListChildBlock.Count > 0)
        {
            gamePlayManager.CheckFirt(control.ListAni[0].BlockStart);
        }
        control.EndAnimation();
        control.IsRun = false;
        
        // control.EndAnimation();
        //HandleScore(blockEnd);
    }

  /*  private void HandleScore(BlockControl blockEnd)
    {
         
         if(control.ListAni.Count > 0 )
            control.ListAni.RemoveAt(0);
        
        if (gamePlayManager.CheckScore(blockEnd) >= gamePlayManager.MunberBlock && !gamePlayManager.StartScaleScore)
        {
            gamePlayManager.StartScaleScore = true;
            uiManager.SetActiveScale(true);
            uiManager.SetActiveTextScale(true);
        }

        if (gamePlayManager.StartScaleScore)
        {
            gamePlayManager.CountScaleScore++;
            float value = (gamePlayManager.CountScaleScore % 5) / 5f;
            uiManager.SetScoreValue(value);
            uiManager.SetTextScale("x" + (gamePlayManager.CountScaleScore / 5 + 1).ToString());
        }
         gamePlayManager.SortAll();
    }
*/
    public void AniStartButton(RectTransform transform)
    {
        transform.DOScale(1.2f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo) 
            .SetEase(Ease.InOutSine);  
    }

    public IEnumerator WaitBack(List<Transform> children, float time)
    {
        yield return new WaitForSeconds(time);
        Booling.ObjectBack(children);
    }
    public IEnumerator PlusScore(BlockControl block, int score, float delay)
    {
        gamePlayManager.StartScaleScore = false;
        block.ListChildBlock.RemoveRange(0, score);
        yield return new WaitForSeconds(delay);

        List<Transform> children = new List<Transform>();

        for (int i = block.transform.childCount - 1; i >= block.transform.childCount - score; i--)
        {
                Transform child = block.transform.GetChild(i);
                // Thêm vào list
                children.Add(child);
        }

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] != null)
                yield return children[i].DOScale(Vector3.zero, 0.1f).WaitForCompletion();
        }

        // Khi vòng lặp xong, các bước tiếp theo chạy
        StartCoroutine(WaitBack(children, 0.1f * children.Count));
        uiManager.SetActiveScale(false);
        StartCoroutine(WaitBack(children, 0.5f* score));
        RectTransform TransformText = uiManager.GetTransformTextSale();
        TextMeshProUGUI textSale = uiManager.GetTextSale();
        Vector2 startPos = TransformText.anchoredPosition;
        float startFontSize = textSale.fontSize;
        foreach (var i in gamePlayManager.DelayCheck)
        {
            gamePlayManager.CheckFirt(i);
        }
/*      
        TransformText.DOAnchorPos(new Vector2(400, 520), 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                uiManager.SetTextScale("+" + (gamePlayManager.ScorePluss * ((gamePlayManager.CountScaleScore / 5) + 1)).ToString());
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

                                gamePlayManager.UpdateScore();
                            });
                    });
            });*/
        control.ScorePlus = false;
       
    }
}
