using UnityEngine;
using DG.Tweening; // DOTween 사용을 위해 필수

public class FloatingEffect : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("얼마나 높이 둥둥 뜰 것인지 거리")]
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
            // 개발자님이 작성하신 코드 적용
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + FloatDistance, Duration)
                         .SetEase(Ease.InOutSine)
                         .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            Debug.LogWarning("FloatingEffect는 UI 요소(RectTransform)에만 사용할 수 있습니다!");
        }
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 찌꺼기 애니메이션이 남지 않도록 깔끔하게 종료해 줍니다.
        if (rectTransform != null)
        {
            rectTransform.DOKill();
        }
    }
}