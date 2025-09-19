using AssetKits.ParticleImage;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Lean.Pool.LeanGameObjectPool;

public class VictoryUI : UICanvas
{

    [SerializeField] Button RePlayButton;
    [SerializeField] Button NextLevelButton;
    [SerializeField] Button HomeButton;
    [SerializeField] DOTweenAnimation VitoryPanel;
    [SerializeField]
    TextMeshProUGUI NumberCoin;
    [SerializeField] TextMeshProUGUI CurrenCoinText;
    [SerializeField] List<ParticleSystem> EffectVitory;
    [SerializeField] List<GameObject> GameObjectWin;
    [SerializeField] ParticleImage CoinMoveEffect;
    private void Start()
    {
        RePlayButton.onClick.AddListener(Replay);
        NextLevelButton.onClick.AddListener(NextLevel);
        HomeButton.onClick.AddListener(GoBackHome);
    }
    IEnumerator DelayClick(Button btn)
    {
        btn.enabled = false;
        yield return new WaitForSeconds(1f);
        btn.enabled = true;
    }
    public override void Open()
    {
        base.Open();
        SetNumberReward();
        SetActiveEventButton(false);
        PlayEffect();
        int lastcoin = Currency.Ins.DataCurrency.coin;
        CurrenCoinText.text = lastcoin.ToString();
        GamePlayManager.Ins.SetPause(true);
    }
    public override void Close(float delayTime)
    {

        base.Close(delayTime);
    }
    public void SetNumberReward()
    {
        NumberCoin.text = GameManager.Ins.GameLevelDataBase.ListGameLevel[GameManager.Ins.CurrenNumberLevel - 1].LevelRewards.NumberItem.ToString();
    }
    public void PlayEffect()
    {
        if (EffectVitory != null && EffectVitory.Count >= 5) {
            StartCoroutine(WaitPlayParticle(0.3f, EffectVitory[0]));
            StartCoroutine(WaitPlayParticle(0.3f, EffectVitory[1]));
            StartCoroutine(WaitPlayParticle(1f, EffectVitory[2]));
            StartCoroutine(WaitPlayParticle(1f, EffectVitory[3]));
            StartCoroutine(WaitPlayParticle(1.7f, EffectVitory[4]));
        }
        StartCoroutine(WaitEndEffectWin(2.5f));
    }
    public IEnumerator WaitEndEffectWin(float Time)
    {
        yield return new WaitForSeconds(Time);
        SetActiveEventButton(true);
        CoinMoveEffect.Play();
    }
    public void EffectPlusCoin()
    {
        int lastcoin = Currency.Ins.DataCurrency.coin;
        Currency.Ins.AddCoin(GameManager.Ins.GameLevelDataBase.ListGameLevel[GameManager.Ins.CurrenNumberLevel - 1].LevelRewards.NumberItem);
        int CurrenCoin = Currency.Ins.DataCurrency.coin;
        AnimateMoney(lastcoin, CurrenCoin);
    }
    public void SetActiveEventButton(bool b)
    {
        foreach (var item in GameObjectWin) {
            item.SetActive(b);
        }
    }
    public IEnumerator WaitPlayParticle(float Time, ParticleSystem Effect)
    {
        yield return new WaitForSeconds(Time);
        Effect.gameObject.SetActive(true);
    }
    public void AnimateMoney(int currentMoney, int targetMoney, float duration = 1f)
    {
        DOTween.To(() => currentMoney, x => {
            currentMoney = x;
            CurrenCoinText.text = currentMoney.ToString();
        }, targetMoney, duration).SetEase(Ease.Linear);
    }
    void Replay()
    {
        StartCoroutine(DelayClick(RePlayButton));
        GameManager.Ins.Replay();
        Close(0f);
        //GamePlayManager.Ins.SetPause(false);    
    }
    void NextLevel()
    {
        StartCoroutine(DelayClick(NextLevelButton));
        GameManager.Ins.NextLevel();
        Close(1f);
        //  GamePlayManager.Ins.SetPause(false);
    }
    public IEnumerator WaitOpenHomeUI(){
        yield return new WaitForSeconds(1);
         UIManager.Ins.GetUI<HomeUI>().Open();
}

    void GoBackHome()
    {
        StartCoroutine(DelayClick(HomeButton));
        GameManager.Ins.BackToHome();
        Close(1f);
       StartCoroutine(WaitOpenHomeUI());
       // GamePlayManager.Ins.SetPause(false);
    }
}
