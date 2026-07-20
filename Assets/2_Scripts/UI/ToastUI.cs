using DG.Tweening;
using System;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float DisplayDuration = 2.5f;
    [SerializeField] private float ScaleInDuration = 0.2f;
    [SerializeField] private float ScaleOutDuration = 0.15f;

    private RectTransform Rect;

    private void Awake()                          // Start �� Awake �� ����
    {
        Rect = GetComponent<RectTransform>();     // SetActive(true) ��� ����
    }
    public void Show(Action onComplete = null)
    {
        gameObject.SetActive(true);
        if (Rect == null) Rect = GetComponent<RectTransform>();
        if (Rect == null) return;
        Rect.DOKill();
        Rect.localScale = Vector3.zero;
        Rect.DOScale(Vector3.one, ScaleInDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(DisplayDuration, () =>
                {
                    Rect.DOScale(Vector3.zero, ScaleOutDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        Rect.localScale = Vector3.zero;
                        onComplete?.Invoke();
                    });
                });
            });
    }
}
