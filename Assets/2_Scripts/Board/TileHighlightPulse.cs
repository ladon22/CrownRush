using DG.Tweening;
using UnityEngine;

public class TileHighlightPulse : MonoBehaviour
{
    [Header("Alpha Settings")]
    public float MinAlpha = 0.3f;
    public float MaxAlpha = 0.8f;

    [Header("Scale Settings")]
    [Tooltip("커질 때 타일 밖으로 얼마나 튀어나갈지 비율 (1.1 = 110%)")]
    public float MaxScaleRatio = 1.1f;

    [Header("Animation Settings")]
    public float AnimDuration = 0.5f;

    private SpriteRenderer SR;

    void Start()
    {
        BoardRenderer boardRenderer = FindFirstObjectByType<BoardRenderer>();
        SR = GetComponent<SpriteRenderer>();

        Vector3 baseScale = Vector3.one;

        if (boardRenderer != null)
        {
            Vector2 targetSize = boardRenderer.GetTileSize();
            transform.localScale = Vector3.one;
            Vector2 currentSize = SR.bounds.size;

            if (currentSize.x > 0 && currentSize.y > 0)
            {
                baseScale = new Vector3(targetSize.x / currentSize.x, targetSize.y / currentSize.y, 1f);
            }
        }

        transform.localScale = baseScale;

        Color color = SR.color;
        color.a = MinAlpha;
        SR.color = color;

        transform.DOScale(baseScale * MaxScaleRatio, AnimDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        SR.DOFade(MaxAlpha, AnimDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void OnDestroy()
    {
        transform.DOKill();
        if (SR != null)
        {
            SR.DOKill();
        }
    }
}
