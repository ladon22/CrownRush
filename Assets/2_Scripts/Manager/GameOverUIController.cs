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

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    // PlayerTurn은 매 턴마다 반복되는 상태라 여기서 처리하면 Hide()가 매턴 호출되어
    // PopUIBlock()이 과도하게 실행됩니다. 부활(OnReviveSuccess) 시의 Hide()는
    // GameManager에서 그대로 직접 호출합니다.
    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Lobby || state == GameState.Initialize)
            Hide();
    }

    public void Show(int currentScore, int bestScore, bool bCanRevive)
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushUIBlock();

        gameObject.SetActive(true);

        if (AdReviveButton != null)
        {
            bool adReady = AdManager.Instance != null && AdManager.Instance.bIsAdReady;
            AdReviveButton.gameObject.SetActive(bCanRevive && adReady);
        }

        if (ScoreTextEvent != null)
        {
            ScoreTextEvent.StringReference.Arguments = new object[] { currentScore };
            ScoreTextEvent.RefreshString(); // "�ܾ��� ���ΰ�ħ ����!" ��� ����
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
