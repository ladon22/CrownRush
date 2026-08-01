using UnityEngine;
using DG.Tweening; // DOTween ����� ���� �ʼ�

public class FloatingEffect : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("�󸶳� ���� �յ� �� ������ �Ÿ�")]
    public float FloatDistance = 20f;

    [Tooltip("�� �� �ö󰡴� �� �ɸ��� �ð�")]
    public float Duration = 1.5f;

    private RectTransform rectTransform;

    void Start()
    {
        // ��ũ��Ʈ�� �پ��ִ� ������Ʈ�� RectTransform�� �ڵ����� �����ɴϴ�.
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            // �����ڴ��� �ۼ��Ͻ� �ڵ� ����
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + FloatDistance, Duration)
                         .SetEase(Ease.InOutSine)
                         .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void OnDestroy()
    {
        // ������Ʈ�� �ı��� �� ��� �ִϸ��̼��� ���� �ʵ��� ����ϰ� ������ �ݴϴ�.
        if (rectTransform != null)
        {
            rectTransform.DOKill();
        }
    }
}