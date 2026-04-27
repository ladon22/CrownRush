using UnityEngine;
using DG.Tweening;

public class UITransitionController : MonoBehaviour
{
    [Header("UI Panels")]
    public RectTransform LobbyUIPanel; // 로비 하단 패널
    public RectTransform GameUIPanel;  // 게임 상단 패널 (점수, 하트 등)

    [Header("Transition Settings")]
    public float SlideDuration = 0.5f;
    public Ease SlideEase = Ease.InOutQuart;

    [Header("Positions")]
    public float LobbyPanelHideOffsetY = -800f; // 로비 패널이 아래로 사라질 때 이동할 거리
    public float GamePanelHideOffsetY = 500f;  // 게임 패널이 위로 사라져 있을 때의 Y 위치

    private Vector2 LobbyPanelOriginPos;
    private Vector2 GamePanelOriginPos;

    private void Awake()
    {
        // 1. 처음 시작할 때 각 패널의 원래 위치(AnchoredPosition)를 기억해 둡니다.
        if (LobbyUIPanel != null) LobbyPanelOriginPos = LobbyUIPanel.anchoredPosition;
        if (GameUIPanel != null) GamePanelOriginPos = GameUIPanel.anchoredPosition;

        // 2. 게임 패널은 화면 위쪽에 숨겨둔 상태로 시작합니다.
        if (GameUIPanel != null)
            GameUIPanel.anchoredPosition = new Vector2(GamePanelOriginPos.x, GamePanelOriginPos.y + GamePanelHideOffsetY);
    }

    // 로비 -> 인게임 전환 연출
    public void SlideToGame()
    {
        if (LobbyUIPanel != null)
        {
            // 로비 패널을 아래로 이동시킵니다.
            LobbyUIPanel.DOAnchorPosY(LobbyPanelOriginPos.y + LobbyPanelHideOffsetY, SlideDuration).SetEase(SlideEase);
        }

        if (GameUIPanel != null)
        {
            // 게임 패널을 원래 위치(아래)로 내려옵니다.
            GameUIPanel.gameObject.SetActive(true); // 활성화 먼저!
            GameUIPanel.DOAnchorPosY(GamePanelOriginPos.y, SlideDuration).SetEase(SlideEase);
        }
    }

    // 인게임 -> 로비 전환 연출
    public void SlideToLobby()
    {
        if (GameUIPanel != null)
        {
            // 게임 패널을 위로 숨깁니다.
            GameUIPanel.DOAnchorPosY(GamePanelOriginPos.y + GamePanelHideOffsetY, SlideDuration).SetEase(SlideEase)
                .OnComplete(() => GameUIPanel.gameObject.SetActive(false)); // 다 올라가면 비활성화
        }

        if (LobbyUIPanel != null)
        {
            // 로비 패널을 원래 위치(위)로 다시 올립니다.
            LobbyUIPanel.gameObject.SetActive(true);
            LobbyUIPanel.DOAnchorPosY(LobbyPanelOriginPos.y, SlideDuration).SetEase(SlideEase);
        }
    }
}