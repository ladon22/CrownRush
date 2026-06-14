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

    private void Awake()                          // Start → Awake 로 변경
    {
        Rect = GetComponent<RectTransform>();     // SetActive(true) 즉시 실행
    }
    public void Show(Action onComplete = null)
    {
        if (Rect == null) return; 
        gameObject.SetActive(true);
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
