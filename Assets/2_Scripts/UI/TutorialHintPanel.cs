using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

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

    private void Awake()
    {
        TutorialHintManager.OnHintRequested += Show;
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        TutorialHintManager.OnHintRequested -= Show;
    }
    private void Show(TutorialHintData data)
    {
        if (TitleText != null)
            TitleText.StringReference.SetReference(TableName, data.TitleKey);

        if (DescriptionText != null)
            DescriptionText.StringReference.SetReference(TableName, data.DescKey);

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

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);
        Time.timeScale = 0f;
        PanelRect.DOKill();
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopInEase).SetUpdate(true); ;

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(() => { });
    }

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
