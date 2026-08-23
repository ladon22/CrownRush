using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro TextComponent;

    [Header("Animation Settings")]
    [SerializeField] private float FloatDistance = 1.0f;
    [SerializeField] private float Duration = 0.6f;

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
        TextComponent.text = $"+ {score}";

        Color color = TextComponent.color;
        color.a = 1f;
        TextComponent.color = color;

        transform.DOKill();
        TextComponent.DOKill();

        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        transform.DOMoveY(transform.position.y + FloatDistance, Duration).SetEase(Ease.OutQuart);

        TextComponent.DOFade(0f, Duration).SetEase(Ease.InCubic).OnComplete(() =>
        {
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