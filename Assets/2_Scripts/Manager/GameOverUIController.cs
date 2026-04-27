using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class GameOverUIController : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform PanelRect;

    [Header("Texts (Localize Events)")]
    public LocalizeStringEvent ScoreTextEvent;
    public LocalizeStringEvent BestScoreTextEvent;

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
        gameObject.SetActive(true);
        bool bIsNewRecord = false;
        int BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (currentScore > BestScore)
        {
            BestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
            bIsNewRecord = true;
        }


        if (ScoreTextEvent != null)
        {
            ScoreTextEvent.StringReference.Arguments = new object[] { currentScore };
            ScoreTextEvent.RefreshString(); // "단어장 새로고침 해줘!" 라고 명령
        }

        // 최고 점수 업데이트
        if (BestScoreTextEvent != null)
        {
            BestScoreTextEvent.StringReference.Arguments = new object[] { BestScore };
            BestScoreTextEvent.RefreshString();
        }

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
