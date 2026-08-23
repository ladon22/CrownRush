using DG.Tweening;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour, IBoardOccupant
{
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }
    protected ItemManager ItemManager;
    [SerializeField]
    private float StartOffset = 0.15f;
    [SerializeField]
    private float StartFadeDuration = 0.5f;
    [SerializeField]
    private float FloatingOffset = 0.15f;
    [SerializeField]
    private float FloatingDuration = 1f;

    private SpriteRenderer ItemSpriteRenderer;

    private void OnDestroy()
    {
        KillTweens();
    }
    public void Initialize(ItemManager ItemManager, int StartX, int StartY, Vector2 WorldPosition)
    {
        this.ItemManager = ItemManager;
        CurrentX = StartX;
        CurrentY = StartY;

        Vector2 StartPosition = new Vector2(WorldPosition.x, WorldPosition.y + StartOffset);
        transform.position = StartPosition;

        ItemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (ItemSpriteRenderer != null)
        {
            Color InitialColor = ItemSpriteRenderer.color;
            InitialColor.a = 0f;
            ItemSpriteRenderer.color = InitialColor;

            ItemSpriteRenderer.DOFade(1f, StartFadeDuration);
        }
        transform.DOMove(WorldPosition, StartFadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(transform.position.y + FloatingOffset, FloatingDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        });
    }

    public void KillTweens()
    {
        transform.DOKill();
        if (ItemSpriteRenderer != null) ItemSpriteRenderer.DOKill();
    }

    public abstract void Consume(PlayerController Player);
}
