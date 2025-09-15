using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public List<RectTransform> TranformButtonBoosters;
    [Header("Introduce")]
    public RectTransform PobUpStart;
    public TextMeshProUGUI TextLevel;
    public TextMeshProUGUI TextScoreIntro;
    [Header("IconPrefab")]
    public Image IconBlockPrefab;

    private void Awake()
    {
       /* SettingButton.enabled = false;
        ReRollButton.enabled = false;
        DestroyBlock.enabled = false;*/
        SettingButton.onClick.AddListener(SettingEvent);
        ReRollButton.onClick.AddListener(ReRollButtonEvent);
        DestroyBlock.onClick.AddListener(EventDestroyBlock);
        CannelBoosters.onClick.AddListener(EventEndBooster);
        ChangeBlockButton.onClick.AddListener(EventChangeBlock);
        TranformButtonBoosters.Add(ReRollButton.GetComponent<RectTransform>());
        TranformButtonBoosters.Add(DestroyBlock.GetComponent<RectTransform>());
        TranformButtonBoosters.Add(ChangeBlockButton.GetComponent<RectTransform>());
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }
    public RectTransform canvasRect;      // Canvas chính  
    private List<RectTransform> spawnedIcons = new List<RectTransform>();

    public void SpawnIConBlock(Vector3 Position, Color color)
    {
        // World 3D → Screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(Position);

        // Screen position → UI local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            out Vector2 uiPos
        );

        // Tạo 1 icon
        RectTransform icon = Instantiate(IconBlockPrefab, canvasRect).GetComponent<RectTransform>();
        icon.anchoredPosition = uiPos;
        IconBlockPrefab.color = color;
        // Random offset để nó rơi lệch
        Vector2 randomOffset = Random.insideUnitCircle * 80f;
        Vector2 targetPos = uiPos + randomOffset + new Vector2(0, -100);

        // Tween rơi xuống
        icon.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad);

        // Lưu để sau gom
        spawnedIcons.Add(icon);
    }

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





    public void SetFillScore(float CurrenScore, float MaxScore)
    {
        FillScore.fillAmount = CurrenScore/MaxScore;
    }
    public void SetTextScore(string TextValue)
    {
        TextScore.text = TextValue;
    }
     public void SettingEvent()
    {
        Close(0f);
        UIManager.Ins.GetUI<SettingUi>().Open();
    }
    public void StartIntro()
    {
        BarTransform.anchoredPosition = new Vector2(0, 300);
        PobUpStart.anchoredPosition = new Vector2(-1500, 0);
        SettingButton.enabled = false;
        GamePlayManager.Ins.SetPause(true);
        StartCoroutine(WaitStartIntro());
    }
    public static void PlayAll(List<GameObject> objects, TweenCallback onComplete = null)
    {
        List<DOTweenAnimation> allAnims = new List<DOTweenAnimation>();

        foreach (var obj in objects)
        {
            if (obj == null) continue;
            DOTweenAnimation[] anims = obj.GetComponents<DOTweenAnimation>();
            allAnims.AddRange(anims);
        }

        if (allAnims.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        Sequence seq = DOTween.Sequence();

        foreach (var anim in allAnims)
        {
            Tween t = anim.tween;
            if (t == null)
            {
                anim.CreateTween(true, true); // tạo tween bên trong component
                t = anim.tween;              // lấy tween đã tạo
            }

            if (t != null) seq.Join(t);
        }

        if (onComplete != null)
            seq.OnComplete(onComplete);

        seq.Play();
    }


    public IEnumerator WaitStartIntro()
    {
        yield return new WaitForSeconds(0.5f);
        PobUpStart.DOAnchorPos3DX(0, 0.7f);
        StartCoroutine(WaitEndIntro());
    }
    public IEnumerator WaitEndIntro()
    {
        yield return new WaitForSeconds(2f);
        BarTransform.DOAnchorPos3DY(-200, 0.7f);
        PobUpStart.DOAnchorPos3DX(1500, 0.7f);
        GamePlayManager.Ins.SetPause(false);
        SettingButton.enabled = true;
        ReRollButton.enabled = true;
        DestroyBlock.enabled = true;
    }
    public void SetupLevel(string nameLevel, string Score)
    {
        TextLevel.text = nameLevel;
        TextScoreIntro.text = Score;
    }
    public override void Open()
    {
        base.Open();
      
       List<GameObject> ObjectAni = new List<GameObject>();
       ObjectAni.Add(SettingButton.gameObject);
       ObjectAni.Add(ReRollButton.gameObject);
       ObjectAni.Add(DestroyBlock.gameObject);
       ObjectAni.Add(ChangeBlockButton.gameObject);
        PlayAll(ObjectAni);
    }
    public void ChangeTextPanel(string tile , string maintext)
    {
        TextTileBoosters.text = tile;
        MainText.text = maintext;
    }
    public void EventDestroyBlock()
    {
        string tile = "Breaker Booster";
        string main = "Choose the block you want to destroy";
        ChangeTextPanel(tile, main);
        GamePlayManager.Ins.SetUpBooster();
        GamePlayManager.Ins.StateBooter = Boosters.DestroyBlock;
    }
    public void EventEndBooster()
    {
        GamePlayManager.Ins.EndBoosters();
        GamePlayManager.Ins.StateBooter = Boosters.None;
    }
    public void EventChangeBlock()
    {
        string tile = "Change position";
        string main = "Hold to move a block you want.";
        ChangeTextPanel(tile, main);
        GamePlayManager.Ins.SetUpBooster();
        GamePlayManager.Ins.StateBooter = Boosters.ChangeBlock;
    }
    public void ReRollButtonEvent()
    {
        GameManager.Ins.Reroll();
    }
    public override void Close(float delayTime)
    {
       
        base.Close(delayTime);
    }
   
}
