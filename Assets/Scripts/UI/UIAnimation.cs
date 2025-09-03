using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimation : MonoBehaviour
{
  /*  [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Animator animator;
    [SerializeField] private BaseButton xBtn;
    [SerializeField] private BaseButton retryBtn;
    [SerializeField] private GameObject heartBreakVFX;

    private void Awake()
    {
        xBtn.AddListener(OnCloseClick);
        retryBtn.AddListener(OnRetryClick);
    }

    public override void Open()
    {
        base.Open();
        animator.gameObject.SetActive(false);
        UIManager.Ins.IsBlockRay = true;
        canvasGroup.DOFade(1f, 0.25f).From(0f).SetEase(Ease.Linear).OnComplete(() =>
        {
            animator.gameObject.SetActive(true);
            animator.Play(Constain.OpenUI);
        });
    }

    public override void Close(float delayTime)
    {
        base.Close(delayTime);
        UIManager.Ins.IsBlockRay = false;
    }

    private void OnCloseClick()
    {
        canvasGroup.DOFade(0f, 0.25f).From(1f).SetEase(Ease.Linear);
        animator.Play(Constain.CloseUI);
        Close(0.25f);
    }*/
}
