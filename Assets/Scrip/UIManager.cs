using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("text")]
    [SerializeField] TextMeshProUGUI CurrenScore;
    [SerializeField] TextMeshProUGUI MaxScore;
    [SerializeField] TextMeshProUGUI TextScale;
    [Header("Slider")]
    [SerializeField] Slider ValueScore;
    [Header("CanvaGroup")]
    [SerializeField] CanvasGroup LoseGame;
    [SerializeField] CanvasGroup StartButton;
    [SerializeField] CanvasGroup PauseButton;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameManager.Instance;
        SetShowUI(PauseButton);
    }
    public void SetScore(int score)
    {
        CurrenScore.text = score.ToString();
    }
    public void SetMaxScore(int maxScore)
    {
        MaxScore.text = maxScore.ToString();
    }
    public void SetScoreValue(float value)
    {
        ValueScore.value = value;
    }
    public void SetActiveScale(bool b)
    {
        if (!b) 
            ValueScore.value = 0;
        
        ValueScore.gameObject.SetActive(b);
    }
    public void SetActiveTextScale(bool b)
    {
        TextScale.gameObject.SetActive(b);
    }
    public void SetTextScale(string value)
    {
        TextScale.text =  value.ToString();
    }
    public RectTransform GetTransformTextSale()
    {
        return TextScale.gameObject.GetComponent<RectTransform>();
    }
    public TextMeshProUGUI GetTextSale()
    {
        return TextScale;
    }
    public TextMeshProUGUI getTextScore()
    {
        return CurrenScore;
    }
    public void SetShowUI(CanvasGroup UI)
    {
        bool show = UI.alpha == 0;

        UI.alpha = show ? 1f : 0f;
        UI.interactable = show;
        UI.blocksRaycasts = show;
    }
    public void Pause()
    {
        gameManager.setPause(true);
    }

    public void Resume()
    {
        gameManager.setPause(false);
    }
    public RectTransform getStartButton()
    {
       return StartButton.GetComponent<RectTransform>();
    }
    public void QuitGame()
    {

        Application.Quit();


#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
    public void Losegame()
    {
        Pause();
        SetShowUI(LoseGame);
    }
}
