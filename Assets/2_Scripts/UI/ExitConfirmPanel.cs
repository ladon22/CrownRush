using DG.Tweening;
using UnityEngine;

/// <summary>
/// 로비에서 뒤로가기를 눌렀을 때 표시되는 종료 확인 팝업.
/// UIBackHandler.HandleDefaultBack()의 Lobby 케이스에서 호출됩니다.
///
/// 다른 패널들과 동일한 패턴(Show/Hide, manageGameState 불필요 ?
/// 로비에서는 timeScale을 건드릴 필요가 없으므로 단순하게 구성)을 따릅니다.
/// </summary>
public class ExitConfirmPanel : MonoBehaviour
{
    [Header("Panel & Animation")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private float PopDuration = 0.3f;
    [SerializeField] private Ease PopEase = Ease.OutBack;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>UIBackHandler에서 호출합니다.</summary>
    public void Show()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushUIBlock();

        gameObject.SetActive(true);

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase);
        }

        // 뒤로가기를 또 누르면 이 팝업이 닫히도록 등록
        // (종료 확인 팝업이 떠있는데 뒤로가기를 누르면 "취소"와 동일하게 처리)
        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(OnCancelButton);
    }

    /// <summary>"취소" 버튼 또는 뒤로가기에 연결합니다.</summary>
    public void OnCancelButton()
    {
        if (PanelRect != null)
        {
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
                .SetEase(Ease.InBack)
                .OnComplete(ClosePanel);
        }
        else
        {
            ClosePanel();
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PopUIBlock();

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    /// <summary>"종료" 버튼에 연결합니다.</summary>
    public void OnConfirmExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
