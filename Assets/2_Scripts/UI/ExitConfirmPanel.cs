using DG.Tweening;
using UnityEngine;

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

    public void Show()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase);
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(OnCancelButton);
    }

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
            GameManager.Instance.InputController.PopInputLock();

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    public void OnConfirmExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
