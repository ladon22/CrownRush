using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

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

    [Header("Revive System")]
    public Button AdReviveButton;

    private const string BestScoreKey = "BestScore";

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(int currentScore, int bestScore, bool bCanRevive)
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushUIBlock();

        gameObject.SetActive(true);

        if (AdReviveButton != null)
            AdReviveButton.gameObject.SetActive(bCanRevive);

        if (ScoreTextEvent != null)
        {
            ScoreTextEvent.StringReference.Arguments = new object[] { currentScore };
            ScoreTextEvent.RefreshString(); // "단어장 새로고침 해줘!" 라고 명령
        }

        if (BestScoreTextEvent != null)
        {
            BestScoreTextEvent.StringReference.Arguments = new object[] { bestScore };
            BestScoreTextEvent.RefreshString();
        }

        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase);
    }

    public void Hide()
    {
        PanelRect.DOKill();
        PanelRect.localScale = Vector3.one;
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopUIBlock();
    }
}
