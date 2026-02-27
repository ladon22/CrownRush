using UnityEngine;

public class TileHighlightPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float PulseSpeed = 5f;
    public float MinAlpha = 0.3f;
    public float MaxAlpha = 0.8f;

    private SpriteRenderer SR;
    private Color OriginalColor;

    void Start()
    {
        BoardRenderer BoardRenderer = FindFirstObjectByType<BoardRenderer>();
        SR = GetComponent<SpriteRenderer>();
        if (BoardRenderer != null )
        {
            Vector2 PrefabSize = BoardRenderer.GetTileSize();
            transform.localScale = PrefabSize;
        }
        OriginalColor = SR.color;

    }

    void Update()
    {
        float Alpha = Mathf.Lerp(MinAlpha, MaxAlpha, (Mathf.Sin(Time.time * PulseSpeed) + 1f) / 2f);
        SR.color = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, Alpha);
    }
}
