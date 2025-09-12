using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        SettingButton.enabled = false;
        ReRollButton.enabled = false;
        DestroyBlock.enabled = false;
        SettingButton.onClick.AddListener(SettingEvent);
        ReRollButton.onClick.AddListener(ReRollButtonEvent);
        DestroyBlock.onClick.AddListener(EventDestroyBlock);
        CannelBoosters.onClick.AddListener(EventEndBooster);
        ChangeBlockButton.onClick.AddListener(EventChangeBlock);

        TranformButtonBoosters.Add(ReRollButton.GetComponent<RectTransform>());
        TranformButtonBoosters.Add(DestroyBlock.GetComponent<RectTransform>());
        TranformButtonBoosters.Add(ChangeBlockButton.GetComponent<RectTransform>());
       
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
