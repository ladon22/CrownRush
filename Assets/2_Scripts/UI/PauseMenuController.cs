using DG.Tweening;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panel & Animation")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private float PopDuration = 0.4f;
    [SerializeField] private Ease PopEase = Ease.OutBack;

    [Header("하위 패널")]
    [SerializeField] private SettingsUIController SettingsPanel;
    [SerializeField] private PieceInfoPanel PieceInfoPanelRef;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        Time.timeScale = 0f;

        gameObject.SetActive(true);

        if (PanelRect != null)
        {
            PanelRect.gameObject.SetActive(true);
            PanelRect.DOKill();
            PanelRect.localScale = Vector3.one;
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(OnContinueButton);
    }

    public void OnContinueButton()
    {
        if (PanelRect != null)
        {
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(CloseMenu);
        }
        else
        {
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopInputLock();

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    public void OnPieceInfoButton()
    {
        if (PieceInfoPanelRef != null)
            PieceInfoPanelRef.Show(manageGameState: false);
    }

    public void OnSettingsButton()
    {
        if (SettingsPanel != null)
            SettingsPanel.Show(manageGameState: false);
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopInputLock();

        gameObject.SetActive(false);

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();

        if (GameManager.Instance != null)
            GameManager.Instance.RetryGame();
    }

    public void OnExitToLobbyButton()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopInputLock();

        gameObject.SetActive(false);

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToLobby();
    }
}
