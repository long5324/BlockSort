using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using static ObjectBoolingControler;
using static Unity.VisualScripting.Metadata;

public class Animation : Singleton<Animation>
{
    [SerializeField] public float TimeUpBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeMoveBlock { get; private set; } = 0.05f;
    [SerializeField] public float TimeDownBlock { get; private set; } = 0.05f;
    private ObjectBoolingControler Booling;
    private AudioControl audioControl;
    private AnimationControl control;
    private GamePlayManager gamePlayManager;
    gamePlayUiManager uiManagerUi;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
        uiManagerUi = gamePlayUiManager.Instance;   
        Booling = ObjectBoolingControler.Instance;
        audioControl = AudioControl.Instance;
        control = AnimationControl.Instance;
        gamePlayManager = GamePlayManager.Instance;
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
            end.transform.childCount * gamePlayManager.sizeYBlock + 0.006f,
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
            audioControl.StartUp();

            bool done = false;

            tf[i].DOLocalMove(new Vector3(pos.x, heightUp + (tf.Count - i) * distanceBlock, pos.z), TimeUpBlock)
                .OnComplete(() => done = true);

            yield return new WaitUntil(() => done);
        }

        // 🚩 Chỗ này bạn đang StartCoroutine
        // StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.05f));

        // ✅ Phải return để giữ flow tuần tự
        yield return StartCoroutine(WaitMove(tf, blockEnd, TimeUpBlock + 0.02f));
    }

    IEnumerator WaitMove(List<Transform> tf, BlockControl blockEnd, float timeWait)
    {
        yield return new WaitForSeconds(timeWait);

        float heightLast = blockEnd.transform.childCount * gamePlayManager.sizeYBlock;
       

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
            audioControl.StartMove();

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

                float newY = lastHeightEnd + ((tf.Count - i) * gamePlayManager.sizeYBlock);
                audioControl.StartDown();

                bool done = false;
                tf[i].DOLocalMove(new Vector3(0, newY, 0), TimeDownBlock)
                     .OnComplete(() => done = true);

                yield return new WaitUntil(() => done);
                yield return new WaitForSeconds(delayBetweenBlocks);
            }

            yield return new WaitForSeconds(TimeDownBlock);

            control.ChangeInDataBlockControl();
        BlockControl BlockStart = control.Ani.BlockStart;
          BlockControl BlockEnd = control.Ani.BlockEnd;
        control.IsRun = false;
        control.EndAnimation();
                foreach(BlockControl i in gamePlayManager.BottomBlock)
        {
            i.UpdateList();
        }
        StartCoroutine(DelayCheck(BlockStart,0.1f));
        StartCoroutine(DelayCheck(BlockEnd, 0.1f));
    }

    IEnumerator DelayCheck(BlockControl BlockList, float Time)
    {
        yield return new WaitForSeconds(Time);
        gamePlayManager.CheckFirt(BlockList);

    }
    /*  private void HandleScore(BlockControl blockEnd)
      {

           if(control.Ani.Count > 0 )
              control.Ani.RemoveAt(0);

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


    public void WaitBack(List<Transform> children)
    {
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
                yield return children[i].DOScale(Vector3.zero, 0.04f).WaitForCompletion();

        }
        gamePlayManager.CurrenScore += children.Count;
        uiManagerUi.Setfill(gamePlayManager.CurrenScore, gameManager.MaxCurrenScore);
        uiManagerUi.ChangeScore(gamePlayManager.CurrenScore.ToString()+"/"+ gameManager.MaxCurrenScore.ToString());
        // Khi vòng lặp xong, các bước tiếp theo chạy
        WaitBack(children);
        if (gamePlayManager.CurrenScore > gameManager.MaxCurrenScore)
        {
            gameManager.Winlevel();
        }
        foreach(BlockControl i in gamePlayManager.BottomBlock)
        {
            i.UpdateList();
        }
        StartCoroutine(DelayCheck(block, 0.1f));
        control.ScorePlus = false;
    }
}
