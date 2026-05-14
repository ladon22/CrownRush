using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro TextComponent;

    [Header("Animation Settings")]
    [SerializeField] private float FloatDistance = 1.0f; // 위로 떠오르는 거리
    [SerializeField] private float Duration = 0.6f;      // 연출 시간

    [Header("Material Settings")]
    [SerializeField] private Material CustomMaterial;

    private void Awake()
    {
        TextComponent = GetComponent<TextMeshPro>();

        if (CustomMaterial != null)
        {
            TextComponent.fontSharedMaterial = CustomMaterial;
        }
    }

    public void Show(int score)
    {
        // "+ 20" 형태로 텍스트 적용
        TextComponent.text = $"+ {score}";

        // 투명도 초기화
        Color color = TextComponent.color;
        color.a = 1f;
        TextComponent.color = color;

        // 기존에 실행 중이던 애니메이션이 있다면 취소 (풀링 재사용을 위함)
        transform.DOKill();
        TextComponent.DOKill();

        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        // 1. 위로 통통 튀어 오르는 이동 애니메이션
        transform.DOMoveY(transform.position.y + FloatDistance, Duration).SetEase(Ease.OutQuart);

        // 2. 서서히 투명해지는 페이드 아웃 애니메이션
        TextComponent.DOFade(0f, Duration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            // 연출이 완전히 끝나면 풀 매니저로 스스로를 반납합니다.
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        });
    }
}