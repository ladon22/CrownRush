using DG.Tweening;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    [HideInInspector]
    public int CurrentX;
    [HideInInspector]
    public int CurrentY;
    protected ItemManager ItemManager;
    [SerializeField]
    private float StartOffset = 0.15f;
    [SerializeField]
    private float StartFadeDuration = 0.5f;
    [SerializeField]
    private float FloatingOffset = 0.15f;
    [SerializeField]
    private float FloatingDuration = 1f;
    
    

    private void OnDestroy()
    {
        transform.DOKill();
    }
    public void Initialize(ItemManager ItemManager, int StartX, int StartY, Vector2 WorldPosition)
    {
        this.ItemManager = ItemManager;
        CurrentX = StartX;
        CurrentY = StartY;

        Vector2 StartPosition = new Vector2(WorldPosition.x, WorldPosition.y + StartOffset);
        transform.position = StartPosition;

        SpriteRenderer SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (SpriteRenderer != null)
        {
            Color InitialColor = SpriteRenderer.color;
            InitialColor.a = 0f;
            SpriteRenderer.color = InitialColor;

            SpriteRenderer.DOFade(1f, StartFadeDuration);
        }
        transform.DOMove(WorldPosition, StartFadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(transform.position.y + FloatingOffset, FloatingDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        });
    }

    public abstract void Consume(PlayerController Player);
}
