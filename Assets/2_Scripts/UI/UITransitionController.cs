using UnityEngine;
using DG.Tweening;

public class UITransitionController : MonoBehaviour
{
    [Header("UI Panels")]
    public RectTransform LobbyTopUIPanel;
    public RectTransform LobbyUIPanel;
    public RectTransform GameUIPanel; 

    [Header("Transition Settings")]
    public float SlideDuration = 0.5f;
    public Ease SlideEase = Ease.InOutQuart;

    [Header("Positions")]
    public float LobbyPanelHideOffsetY = -800f;
    public float GamePanelHideOffsetY = 500f;

    private Vector2 LobbyTopPanelOriginPos;
    private Vector2 LobbyPanelOriginPos;
    private Vector2 GamePanelOriginPos;

    private void Awake()
    {
        if (LobbyTopUIPanel != null) LobbyTopPanelOriginPos = LobbyTopUIPanel.anchoredPosition;
        if (LobbyUIPanel != null) LobbyPanelOriginPos = LobbyUIPanel.anchoredPosition;
        if (GameUIPanel != null) GamePanelOriginPos = GameUIPanel.anchoredPosition;

        if (GameUIPanel != null)
            GameUIPanel.anchoredPosition = new Vector2(GamePanelOriginPos.x, GamePanelOriginPos.y + GamePanelHideOffsetY);
    }
    public void SlideToGame()
    {
        if (LobbyUIPanel != null)
        {
            LobbyUIPanel.DOAnchorPosY(LobbyPanelOriginPos.y + LobbyPanelHideOffsetY, SlideDuration).SetEase(SlideEase);
        }

        if (LobbyTopUIPanel != null)
        {
            LobbyTopUIPanel.DOAnchorPosY(LobbyTopPanelOriginPos.y - LobbyPanelHideOffsetY, SlideDuration).SetEase(SlideEase);
        }

        if (GameUIPanel != null)
        {
            GameUIPanel.DOAnchorPosY(GamePanelOriginPos.y, SlideDuration).SetEase(SlideEase);
        }
    }

    public void SlideToLobby()
    {
        if (GameUIPanel != null)
        {
            GameUIPanel.DOAnchorPosY(GamePanelOriginPos.y + GamePanelHideOffsetY, SlideDuration).SetEase(SlideEase);
        }

        if (LobbyUIPanel != null)
        {
            LobbyUIPanel.DOAnchorPosY(LobbyPanelOriginPos.y, SlideDuration).SetEase(SlideEase);
        }

        if (LobbyTopUIPanel != null)
        {
            LobbyTopUIPanel.DOAnchorPosY(LobbyTopPanelOriginPos.y, SlideDuration).SetEase(SlideEase);
        }
    }
}