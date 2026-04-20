using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameOverUIController : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform PanelRect;

    [Header("Texts")]
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI BestScoreText;

    [Header("Animation Settings")]
    public float PopDuration = 0.4f;
    public Ease PopEase = Ease.OutBack;

    private const string BestScoreKey = "BestScore";

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public bool Show(int currentScore)
    {
        bool bIsNewRecord = false;
        int BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (currentScore > BestScore)
        {
            BestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
            bIsNewRecord = true;
        }

        if (ScoreText != null) ScoreText.text = "Score: " + currentScore.ToString();
        if (BestScoreText != null) BestScoreText.text ="Best: " + BestScore.ToString();

        gameObject.SetActive(true);
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase);
        return bIsNewRecord;
    }

    public void Hide()
    {
        PanelRect.DOKill();
        PanelRect.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
}
