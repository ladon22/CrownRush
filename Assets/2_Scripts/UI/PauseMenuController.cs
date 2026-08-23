using DG.Tweening;
using UnityEngine;

/// <summary>
/// 인게임 일시정지 메뉴.
///
/// 톱니바퀴 버튼 클릭 시 이 패널이 열립니다.
///
/// 핵심 설계 — timeScale/InputLock 책임 분리
///   PauseMenuController만 Time.timeScale과 PushInputLock/PopInputLock을 관리합니다.
///   하위 패널(Settings, PieceInfo)은 PauseMenu에서 열릴 때
///   manageGameState: false로 호출되어 timeScale/InputLock을 건드리지 않습니다.
///   (이미 PauseMenu가 게임을 멈추고 입력을 차단한 상태이므로 중복 처리 방지)
/// </summary>
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

    /// <summary>톱니바퀴 버튼 OnClick에 연결합니다.</summary>
    public void Show()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        // [핵심] 일시정지 메뉴가 열리는 시점에 게임 시간을 멈춤
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

    /// <summary>"이어하기" 버튼에 연결합니다.</summary>
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

        // [핵심] 게임 재개는 여기서만 처리 — 하위 패널은 건드리지 않음
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopInputLock();

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    /// <summary>"적 정보" 버튼에 연결합니다.</summary>
    public void OnPieceInfoButton()
    {
        if (PieceInfoPanelRef != null)
            PieceInfoPanelRef.Show(manageGameState: false); // PauseMenu가 이미 처리함
    }

    /// <summary>"설정" 버튼에 연결합니다.</summary>
    public void OnSettingsButton()
    {
        if (SettingsPanel != null)
            SettingsPanel.Show(manageGameState: false); // PauseMenu가 이미 처리함
    }

    /// <summary>"다시 하기" 버튼에 연결합니다.</summary>
    public void OnRetryButton()
    {
        // 메뉴를 닫고 게임 시간을 정상화한 뒤 게임을 재시작
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopInputLock();

        gameObject.SetActive(false);

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();

        if (GameManager.Instance != null)
            GameManager.Instance.RetryGame();
    }

    /// <summary>"로비로 나가기" 버튼에 연결합니다.</summary>
    public void OnExitToLobbyButton()
    {
        // 메뉴를 닫고 게임 시간을 정상화한 뒤 로비로 이동
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
