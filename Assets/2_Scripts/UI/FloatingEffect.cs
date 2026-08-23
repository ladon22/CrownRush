using UnityEngine;
using DG.Tweening;

public class FloatingEffect : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("얼마나 위로 둥실 뜨는지 거리")]
    public float FloatDistance = 20f;

    [Tooltip("한 번 올라가는 데 걸리는 시간")]
    public float Duration = 1.5f;

    private RectTransform RectTransform;

    void Start()
    {
        RectTransform = GetComponent<RectTransform>();

        if (RectTransform != null)
        {
            RectTransform.DOAnchorPosY(RectTransform.anchoredPosition.y + FloatDistance, Duration)
                         .SetEase(Ease.InOutSine)
                         .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void OnDestroy()
    {
        if (RectTransform != null)
        {
            RectTransform.DOKill();
        }
    }
}
