using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button playBtn;
    [SerializeField] Button RightButton;
    [SerializeField] Button LeftButton;
    public TextMeshProUGUI LevelName;
    IEnumerator WaitOffUI(List<CanvasGroup> ListUI)
    {
        yield return new WaitForSeconds(1);
        foreach (CanvasGroup i in ListUI)
        {
            i.alpha = 0;
        }
    }
    private void Awake()
    {
        playBtn.onClick.AddListener(StartGame);
        RightButton.onClick.AddListener(NextLevel);
        LeftButton.onClick.AddListener(PreviousLevel);
    }
    public void EventRightButton()
    {
    }
    public override void Open()
    {
        base.Open();
    }


    public override void Close(float delayTime)
    {
        base.Close(delayTime);
    }
    void StartGame()
    {
        Close(0f);
        GameManager.Ins.SetUpLevel(currentIndex + 1);
    }
    public DataHomeLevel levelData;

    public Image imageFarLeft;
    public Image imageLeft;
    public Image imageCenter;
    public Image imageRight;
    public Image imageFarRight;

    private int currentIndex = 0;

    // Positions for tween
    private Vector3 farLeftPos = new Vector3(-1400, 0, 0);
    private Vector3 leftPos = new Vector3(-500, 0, 0);
    private Vector3 centerPos = Vector3.zero;
    private Vector3 rightPos = new Vector3(500, 0, 0);
    private Vector3 farRightPos = new Vector3(1400, 0, 0);

    private Vector3 centerScale = Vector3.one * 1.2f;
    private Vector3 sideScale = Vector3.one * 0.8f;
    private Vector3 farScale = Vector3.one * 0.5f;

    private void Start()
    {
        UpdateUIImmediate();
    }

    public void NextLevel()
    {
        if (currentIndex < levelData.UIGameLevel.Count - 1)
        {
            currentIndex++;
            AnimateUI(true);
            LevelName.text = "Level " + (currentIndex+1).ToString(); 
        }
    }

    public void PreviousLevel()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            AnimateUI(false);
            LevelName.text = "Level " + (currentIndex+1).ToString();
        }
    }

    private void UpdateUIImmediate()
    {
        // Center
        imageCenter.sprite = levelData.UIGameLevel[currentIndex];
        imageCenter.transform.localPosition = centerPos;
        imageCenter.transform.localScale = centerScale;
        imageCenter.color = Color.white;

        // Left
        imageLeft.sprite = currentIndex > 0 ? levelData.UIGameLevel[currentIndex - 1] : null;
        imageLeft.transform.localPosition = leftPos;
        imageLeft.transform.localScale = sideScale;
        imageLeft.color = new Color(1, 1, 1, 0.6f);
        imageLeft.gameObject.SetActive(imageLeft.sprite != null);

        // Far Left
        imageFarLeft.sprite = currentIndex > 1 ? levelData.UIGameLevel[currentIndex - 2] : null;
        imageFarLeft.transform.localPosition = farLeftPos;
        imageFarLeft.transform.localScale = farScale;
        imageFarLeft.color = new Color(1, 1, 1, 0.3f);
        imageFarLeft.gameObject.SetActive(imageFarLeft.sprite != null);

        // Right
        imageRight.sprite = currentIndex < levelData.UIGameLevel.Count - 1 ? levelData.UIGameLevel[currentIndex + 1] : null;
        imageRight.transform.localPosition = rightPos;
        imageRight.transform.localScale = sideScale;
        imageRight.color = new Color(1, 1, 1, 0.6f);
        imageRight.gameObject.SetActive(imageRight.sprite != null);

        // Far Right
        imageFarRight.sprite = currentIndex < levelData.UIGameLevel.Count - 2 ? levelData.UIGameLevel[currentIndex + 2] : null;
        imageFarRight.transform.localPosition = farRightPos;
        imageFarRight.transform.localScale = farScale;
        imageFarRight.color = new Color(1, 1, 1, 0.3f);
        imageFarRight.gameObject.SetActive(imageFarRight.sprite != null);
    }

    private void AnimateUI(bool next)
    {
        float duration = 0.3f;

        if (next)
        {
            // Tween positions & scales
            imageFarLeft.transform.DOLocalMove(farLeftPos, duration).SetEase(Ease.InOutSine);
            imageLeft.transform.DOLocalMove(farLeftPos, duration).SetEase(Ease.InOutSine).OnComplete(() => SwapImages(true));
            imageCenter.transform.DOLocalMove(leftPos, duration).SetEase(Ease.InOutSine);
            imageRight.transform.DOLocalMove(centerPos, duration).SetEase(Ease.InOutSine);
            imageFarRight.transform.DOLocalMove(rightPos, duration).SetEase(Ease.InOutSine);

            // Scale & alpha
            imageCenter.transform.DOScale(sideScale, duration);
            imageRight.transform.DOScale(centerScale, duration);
            imageLeft.transform.DOScale(farScale, duration);
            imageFarRight.transform.DOScale(sideScale, duration);

            imageLeft.DOFade(0.3f, duration);
            imageFarRight.DOFade(0.6f, duration);
            imageRight.DOFade(1f, duration);
            imageCenter.DOFade(0.6f, duration);

        }
        else
        {
            // Tween positions & scales
            imageFarRight.transform.DOLocalMove(farRightPos, duration).SetEase(Ease.InOutSine);
            imageRight.transform.DOLocalMove(farRightPos, duration).SetEase(Ease.InOutSine).OnComplete(() => SwapImages(false));
            imageCenter.transform.DOLocalMove(rightPos, duration).SetEase(Ease.InOutSine);
            imageLeft.transform.DOLocalMove(centerPos, duration).SetEase(Ease.InOutSine);
            imageFarLeft.transform.DOLocalMove(leftPos, duration).SetEase(Ease.InOutSine);

            // Scale & alpha
            imageCenter.transform.DOScale(sideScale, duration);
            imageLeft.transform.DOScale(centerScale, duration);
            imageRight.transform.DOScale(farScale, duration);
            imageFarLeft.transform.DOScale(sideScale, duration);

            imageRight.DOFade(0.3f, duration);
            imageFarLeft.DOFade(0.6f, duration);
            imageLeft.DOFade(1f, duration);
            imageCenter.DOFade(0.6f, duration);
        }
    }

    private void SwapImages(bool next)
    {
        if (next)
        {
            // Shift references to rotate images
            Image temp = imageFarLeft;
            imageFarLeft = imageLeft;
            imageLeft = imageCenter;
            imageCenter = imageRight;
            imageRight = imageFarRight;
            imageFarRight = temp;
        }
        else
        {
            Image temp = imageFarRight;
            imageFarRight = imageRight;
            imageRight = imageCenter;
            imageCenter = imageLeft;
            imageLeft = imageFarLeft;
            imageFarLeft = temp;
        }

        // Update sprites after swap
        UpdateUIImmediate();
    }
}
