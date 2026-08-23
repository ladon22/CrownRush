using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 힌트 팝업 UI.
///
/// TutorialHintManager를 직접 참조하지 않습니다.
/// OnEnable/OnDisable에서 이벤트를 구독/해제하고
/// 이벤트가 오면 스스로 표시 처리를 담당합니다.
/// </summary>
public class TutorialHintPanel : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private LocalizeStringEvent TitleText;
    [SerializeField] private LocalizeStringEvent DescriptionText;
    [SerializeField] private Image IllustrationImage;

    [Header("로컬라이제이션 테이블")]
    [SerializeField] private string TableName = "UI_Text";

    [Header("애니메이션")]
    [SerializeField] private float PopDuration = 0.3f;
    [SerializeField] private Ease PopInEase = Ease.OutBack;
    [SerializeField] private Ease PopOutEase = Ease.InBack;

    // ── 이벤트 구독 / 해제 ───────────────────────────────────

    private void Awake()
    {
        TutorialHintManager.OnHintRequested += Show;
        gameObject.SetActive(false);
    }


    private void OnDestroy()
    {
        TutorialHintManager.OnHintRequested -= Show;
    }

    // ── 표시 / 닫기 ──────────────────────────────────────────

    /// <summary>
    /// OnHintRequested 이벤트를 받아 패널을 표시합니다.
    /// TutorialHintManager가 직접 호출하지 않습니다.
    /// </summary>
    private void Show(TutorialHintData data)
    {
        // 제목, 설명 설정
        if (TitleText != null)
            TitleText.StringReference.SetReference(TableName, data.TitleKey);

        if (DescriptionText != null)
            DescriptionText.StringReference.SetReference(TableName, data.DescKey);

        // 일러스트 — 있으면 표시, 없으면 숨김
        if (IllustrationImage != null)
        {
            bool hasIllustration = data.Illustration != null;
            IllustrationImage.gameObject.SetActive(hasIllustration);
            if (hasIllustration)
            {
                IllustrationImage.sprite = data.Illustration;
                IllustrationImage.type = Image.Type.Simple;
                IllustrationImage.preserveAspect = true;
            }
        }

        // 입력 차단
        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);
        Time.timeScale = 0f;
        // 등장 애니메이션 (크기 0 → 1)
        PanelRect.DOKill();
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopInEase).SetUpdate(true); ;

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(() => { });
    }

    /// <summary>확인 버튼에 연결합니다.</summary>
    public void OnConfirmButton()
    {
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
            .SetEase(PopOutEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                Time.timeScale = 1f;

                if (GameManager.Instance != null)
                    GameManager.Instance.InputController.PopInputLock();

                if (TutorialHintManager.Instance != null)
                    TutorialHintManager.Instance.OnHintConfirmed();

                if (UIBackHandler.Instance != null)
                    UIBackHandler.Instance.Pop();
            });
    }
}
