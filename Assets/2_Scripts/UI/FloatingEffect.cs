using UnityEngine;
using DG.Tweening; // DOTween 사용을 위해 필수

public class FloatingEffect : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("얼마나 위로 둥실 뜨는지 거리")]
    public float FloatDistance = 20f;

    [Tooltip("한 번 올라가는 데 걸리는 시간")]
    public float Duration = 1.5f;

    private RectTransform rectTransform;

    void Start()
    {
        // 스크립트가 붙어있는 오브젝트의 RectTransform을 자동으로 가져옵니다.
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            // 위아래로 둥실거리는 트윈 애니메이션
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + FloatDistance, Duration)
                         .SetEase(Ease.InOutSine)
                         .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 남은 애니메이션이 계속되지 않도록 트윈을 정리합니다.
        if (rectTransform != null)
        {
            rectTransform.DOKill();
        }
    }
}
