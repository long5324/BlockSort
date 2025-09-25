using AssetKits.ParticleImage;
using DG.Tweening;
using HumanSort;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class GameplayUI : UICanvas
{
    [SerializeField] Image FillScore;
    [SerializeField] TextMeshProUGUI TextScore;
    public Button SettingButton;
    public Button ReRollButton;
    public Button DestroyBlock;
    public Button ChangeBlockButton;
    public RectTransform BarTransform;

    [Header("Introduce Boosters")]
    public RectTransform PanelIntroduceBoosters;
    public Button CannelBoosters;
    public TextMeshProUGUI TextTileBoosters;
    public TextMeshProUGUI MainText;
    public TextMeshProUGUI PriceDestroyBooters;
    public TextMeshProUGUI PriceChangeBlockBooters;
    public TextMeshProUGUI PriceRoolBooters;
    public List<RectTransform> TranformButtonBoosters;
    [Header("Introduce")]
    public RectTransform PobUpStart;
    public TextMeshProUGUI TextLevel;
    public TextMeshProUGUI TextScoreIntro;
    [Header("IconPrefab")]
    public Image IconBlockPrefab;
    [Header("Data Coin")]
    public RectTransform CoinDataRec;
    public TextMeshProUGUI CoinDataText;
    [Header("Booters")]
    public TextMeshProUGUI NumberDestroyBlock_Booters;
    public TextMeshProUGUI NumberChangeBlock_Booters;
    public TextMeshProUGUI NumberRool_Booters;
    bool BoxCoinDataOpen = false;
    private void Awake()
    {
        SettingButton.onClick.AddListener(SettingEvent);
        ReRollButton.onClick.AddListener(ReRollButtonEvent);
        DestroyBlock.onClick.AddListener(EventDestroyBlock);
        CannelBoosters.onClick.AddListener(EventEndBooster);
        ChangeBlockButton.onClick.AddListener(EventChangeBlock);
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }
    public RectTransform canvasRect;      // Canvas chính  
    private List<RectTransform> spawnedIcons = new List<RectTransform>();

  

    public void CollectAllIcons(RectTransform targetUI, System.Action onComplete = null)
    {
        // Lấy screen position của targetUI
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            targetUI.position
        );

        // Đổi sang local position trong canvasRect
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            out Vector2 targetPos
        );

        int remaining = spawnedIcons.Count;

        foreach (var icon in spawnedIcons)
        {
            if (icon == null)
            {
                remaining--;
                continue;
            }

            icon.DOAnchorPos(targetPos, 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (icon != null) Destroy(icon.gameObject);

                    remaining--;
                    if (remaining <= 0)
                    {
                        // Chỉ clear khi tất cả icon đã xong
                        spawnedIcons.Clear();
                        onComplete?.Invoke();
                    }
                });
        }

        // ❌ Đừng clear list ở đây nữa
        // spawnedIcons.Clear();
    }
    private int currentDisplayedScore = 0; // giữ số hiện tại

    public void SetScore(float currentScore, float maxScore, float duration = 0.5f)
    {
        float targetFill = currentScore / maxScore;

        // Kill tween cũ nếu có để tránh chồng
        DOTween.Kill(FillScore);
        DOTween.Kill(TextScore);

        // Animate fill
        FillScore.DOFillAmount(targetFill, duration)
                 .SetEase(Ease.OutCubic)
                 .SetId(FillScore);

        // Animate text (Current/Max)
        int endValue = Mathf.RoundToInt(currentScore);

        DOTween.To(() => currentDisplayedScore, x =>
        {
            currentDisplayedScore = x;
            TextScore.text = $"{x}/{Mathf.RoundToInt(maxScore)}";
        }, endValue, duration).SetEase(Ease.OutCubic)
          .SetId(TextScore);
    }
    public void ActionAffterIntrol()
    {
        if (GameManager.Ins.CurrenNumberLevel != 1) return;

        Transform levelGame = GameManager.Ins.LevelGame.transform;

        // Kiểm tra số child trước
        if (levelGame.childCount > 1 && levelGame.GetChild(1).childCount > 1)
        {
            Vector3 startPos = levelGame.GetChild(1).GetChild(1).position;
            Vector3 endPos = GameManager.Ins.CurrenGridLevel.ListblockGround[5].transform.position;

            Tutorial.Ins.WorldDrag(
                startPos,
                endPos,
                Vector3.zero,
                1.3f // thời gian di chuyển tay
            );

            GamePlayManager.Ins.TutorialActive = true;
        }
        else
        {
            Debug.LogWarning("LevelGame không có đủ child để chạy tutorial!");
        }
    }

    public void OpenCoinData()
    {
        if(CoinDataRec == null) { Debug.Log("Coin Data null"); return; }
        BoxCoinDataOpen = true;
        CoinDataText.text = Currency.Ins.DataCurrency.coin.ToString();
        CoinDataRec.DOAnchorPosX(130,0.4f).SetEase(Ease.OutQuad);
    }
    public void CloseCoinData()
    {
        if (CoinDataRec == null) { Debug.Log("Coin Data null"); return; }
        CoinDataRec.DOAnchorPosX(-270, 0.4f).SetEase(Ease.OutQuad);
    }
    public IEnumerator ShakeUI(float timeWait, RectTransform UIRect)
    {
        yield return new WaitForSeconds(timeWait);
        Vector3 originalPos = UIRect.anchoredPosition;
        UIRect.DOShakeAnchorPos(
            duration: 0.5f,
            strength: new Vector2(10f, 10f),
            vibrato: 10,
            randomness: 90f
        ).OnComplete(() => UIRect.anchoredPosition = originalPos);
    }
    public void UpdateBooters()
    {
        int NumberDestroy = Currency.Ins.GetBooster(BootersName.DestroyBlock);
        NumberDestroyBlock_Booters.text = NumberDestroy.ToString();
        int NumberChange = Currency.Ins.GetBooster(BootersName.ChangeBlock);
        NumberChangeBlock_Booters.text = NumberChange.ToString();
        int NumberRool = Currency.Ins.GetBooster(BootersName.Rool);
        NumberRool_Booters.text = NumberRool.ToString();
    }
    private void UpdateColorPriceBooters()
    {
        int CurrenCoin = Currency.Ins.DataCurrency.coin;
        if(CurrenCoin < 500)
        {
            PriceChangeBlockBooters.color = Color.red;
            PriceDestroyBooters.color = Color.red;
        }
        else
        {
            PriceChangeBlockBooters.color = Color.white;
            PriceDestroyBooters.color = Color.white;
            PriceRoolBooters.color = Color.white;
        }
        if (CurrenCoin < 200)
        {
            PriceRoolBooters.color = Color.red;
        }
        else
        {
            PriceRoolBooters.color = Color.white;
        }
    }
    public IEnumerator StartCoinEffect(int currentMoney, int targetMoney)
    {
        yield return new WaitForSeconds(0.6f);
        AnimateMoney(currentMoney, targetMoney);
    }
    public IEnumerator WaitCloseCoinData()
    {
        yield return new WaitForSeconds(3);
        CloseCoinData();
        BoxCoinDataOpen = false ;
    }
    public void AnimateMoney(int currentMoney, int targetMoney, float duration = 1f)
    {
        DOTween.To(() => currentMoney, x => {
            currentMoney = x;
            CoinDataText.text = currentMoney.ToString();
        }, targetMoney, duration).SetEase(Ease.Linear);
        Currency.Ins.SetDataCoin(targetMoney);
    }
    public void SettingEvent()
    {
        Close(0f);
        UIManager.Ins.GetUI<SettingUi>().Open();
    }
    Coroutine CloseCocoutin = null;
    public void SetCoinEffect(int CoinPay)
    {
        CoinDataText.text = Currency.Ins.DataCurrency.coin.ToString();
        int CurrenCoin = Currency.Ins.DataCurrency.coin;
        int NextCoin = CurrenCoin - CoinPay;
        OpenCoinData();
        StartCoroutine(StartCoinEffect(CurrenCoin, NextCoin));
        if (CloseCocoutin == null)
        {
            CloseCocoutin = StartCoroutine(WaitCloseCoinData());
        }
        else {
            StopCoroutine(CloseCocoutin);
            CloseCocoutin = StartCoroutine(WaitCloseCoinData());
        }
    }
    public void StartIntro()
    {
        BarTransform.anchoredPosition = new Vector2(0, 300);
        PobUpStart.anchoredPosition = new Vector2(-1500, 0);
        StartCoroutine(WaitStartIntro());
    }
    public bool CheckCoin(float NumberCoinPay, TextMeshProUGUI TextPay)
    {
        if (Currency.Ins.DataCurrency.coin < NumberCoinPay)
        {
            OpenCoinData();
            StartCoroutine(ShakeUI(0.5f, CoinDataRec));
            if (CloseCocoutin == null)
            {
                CloseCocoutin = StartCoroutine(WaitCloseCoinData());
            }
            else
            {
                StopCoroutine(CloseCocoutin);
                CloseCocoutin = StartCoroutine(WaitCloseCoinData());
            }
            return false;
        }
        return true;    
    }

    public IEnumerator WaitStartIntro()
    {
        yield return new WaitForSeconds(0.5f);
        PobUpStart.DOAnchorPos3DX(0, 0.4f).SetEase(Ease.OutBack);
        StartCoroutine(WaitEndIntro());
    }
    public IEnumerator WaitEndIntro()
    {
        yield return new WaitForSeconds(2f);
        BarTransform.DOAnchorPos3DY(-200, 0.4f).SetEase(Ease.OutBack);
        BarTransform.localScale = new Vector3(1.4f, 0.6f, 1f);
        BarTransform.DOScale(Vector3.one, 0.25f)
                  .SetEase(Ease.OutBack);
        // Di chuyển
        PobUpStart.DOAnchorPos3DX(1500, 0.4f).SetEase(Ease.OutBack);

        // Scale
        PobUpStart.localScale = new Vector3(1.4f, 0.6f, 1f);
        PobUpStart.DOScale(Vector3.one, 0.25f)
                  .SetEase(Ease.OutBack);
        GamePlayManager.Ins.SetPause(false);
        SettingButton.enabled = true;
        ReRollButton.enabled = true;
        DestroyBlock.enabled = true;
        ChangeBlockButton.enabled = true;
        ActionAffterIntrol();
    }
    public void SetupLevel(string nameLevel, string Score)
    {
        TextLevel.text = nameLevel;
        TextScoreIntro.text = Score;
    }
    public override void Open()
    {
        base.Open();
        UpdateColorPriceBooters();
        UpdateBooters();
    }
    public void UnClickCButton()
    {
        SettingButton.enabled = false;
        ReRollButton.enabled = false;
        DestroyBlock.enabled = false;
        ChangeBlockButton.enabled = false;
    }
    public void ChangeTextPanel(string tile , string maintext)
    {
        TextTileBoosters.text = tile;
        MainText.text = maintext;
    }
    public void EventDestroyBlock()
    {
        if (!Currency.Ins.UseBooster(BootersName.DestroyBlock))
        {
            if (CheckCoin(500, PriceDestroyBooters))
            {
                UIManager.Ins.GetUI<ByBooters>().Open();
                UIManager.Ins.GetUI<ByBooters>().coinBy = 500;
                UIManager.Ins.GetUI<ByBooters>().ButtonWaintBy = BootersName.DestroyBlock;
                GamePlayManager.Ins.SetPause(true);
                
            }
            return;
        }
        UpdateBooters();
       /* if (!CheckCoin(500, PriceDestroyBooters)) {
            ShakeUI(0f, DestroyBlock.GetComponent<RectTransform>());
            return; 
        }*/
        string tile = "Breaker Booster";
        string main = "Choose the block you want to destroy";
        ChangeTextPanel(tile, main);
        GamePlayManager.Ins.SetUpBooster();
        GamePlayManager.Ins.StateBooter = Boosters.DestroyBlock;
        UpdateColorPriceBooters();
    }
    public void EventChangeBlock()
    {
        if (!Currency.Ins.UseBooster(BootersName.ChangeBlock))
        {
            if (CheckCoin(500, PriceDestroyBooters))
            {
                UIManager.Ins.GetUI<ByBooters>().Open();
                UIManager.Ins.GetUI<ByBooters>().coinBy = 500;
                UIManager.Ins.GetUI<ByBooters>().ButtonWaintBy = BootersName.ChangeBlock;
                GamePlayManager.Ins.SetPause(true);
            }
            return;
        }
        UpdateBooters();
        /*  if (!CheckCoin(500,PriceChangeBlockBooters))
          {
              ShakeUI(0f, ChangeBlockButton.GetComponent<RectTransform>());
              return;
          }*/
        string tile = "Change position";
        string main = "Hold to move a block you want.";
        ChangeTextPanel(tile, main);
        GamePlayManager.Ins.SetUpBooster();
        GamePlayManager.Ins.StateBooter = Boosters.ChangeBlock;
        UpdateColorPriceBooters();
    }
    public void ReRollButtonEvent()
    {
        if (!Currency.Ins.UseBooster(BootersName.Rool)) {
            if (CheckCoin(200, PriceDestroyBooters))
            {
                UIManager.Ins.GetUI<ByBooters>().Open();
                UIManager.Ins.GetUI<ByBooters>().coinBy = 200;
                UIManager.Ins.GetUI<ByBooters>().ButtonWaintBy = BootersName.Rool;
                GamePlayManager.Ins.SetPause(true);
            }

            return;
        }
        UpdateBooters();
        GameManager.Ins.Reroll();
        UpdateColorPriceBooters();
        ReRollButton.enabled = false;
        StartCoroutine(DelayRoolButton());
    }

    IEnumerator DelayRoolButton()
    {
        yield return new WaitForSeconds(0.5f);
        ReRollButton.enabled = true;
    }
    public void EventEndBooster()
    {
        GamePlayManager.Ins.EndBoosters();
        GamePlayManager.Ins.StateBooter = Boosters.None;

    }
    public override void Close(float delayTime)
    {
       
        base.Close(delayTime);
        CoinDataRec.anchoredPosition = new Vector3(-270, CoinDataRec.anchoredPosition.y);
    }
   
}
