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
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(int currentScore, int bestScore, bool canRevive)
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);

        if (AdReviveButton != null)
        {
            bool adReady = AdManager.Instance != null && AdManager.Instance.IsAdReady;
            AdReviveButton.gameObject.SetActive(canRevive && adReady);
        }

        if (ScoreTextEvent != null)
        {
            ScoreTextEvent.StringReference.Arguments = new object[] { currentScore };
            ScoreTextEvent.RefreshString();
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
            GameManager.Instance.InputController.PopInputLock();
    }
}
