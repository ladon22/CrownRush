using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardRenderer : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject BaseTilePrefab;

    [Header("Board Frame Settings")]
    public Image BoardFrameImage;
    public float FramePaddingX = 1.0f;
    public float FramePaddingY = 1.0f;

    private List<SpriteRenderer> LightTiles = new List<SpriteRenderer>();
    private List<SpriteRenderer> DarkTiles = new List<SpriteRenderer>();
    private Sprite DefaultFrameSprite;

    private void Awake()
    {
        if (BoardFrameImage != null)
        {
            DefaultFrameSprite = BoardFrameImage.sprite;
        }
    }

    private void OnEnable()
    {
        AchievementManager.OnSkinEquipped += UpdateBoardSkin;
    }

    private void OnDisable()
    {
        AchievementManager.OnSkinEquipped -= UpdateBoardSkin;
    }

    public Vector2 GetTileSize()
    {
        SpriteRenderer SpriteRenderer = BaseTilePrefab.GetComponent<SpriteRenderer>();
        if (BaseTilePrefab == null || SpriteRenderer == null) return Vector2.one;
        return SpriteRenderer.bounds.size;
    }

    public void GenerateBoardVisuals(GridManager GridManager)
    {
        if (BaseTilePrefab == null || GridManager == null) return;

        LightTiles.Clear();
        DarkTiles.Clear();

        Vector2 tileSize = GetTileSize();

        for (int Y = 0; Y < GridManager.Rows; Y++)
        {
            for (int X = 0; X < GridManager.Columns; X++)
            {
                Vector2 TilePosition = GridManager.GetWorldPosition(X, Y);
                GameObject NewTile = Instantiate(BaseTilePrefab, TilePosition, Quaternion.identity);
                NewTile.transform.SetParent(this.transform);
                NewTile.name = $"Tile_{Y}_{X}";

                SpriteRenderer sr = NewTile.GetComponent<SpriteRenderer>();

                if ((Y + X) % 2 == 0) LightTiles.Add(sr);
                else DarkTiles.Add(sr);
            }
        }

        if (BoardFrameImage != null)
        {
            float totalWidth = (GridManager.Columns * tileSize.x) + FramePaddingX;
            float totalHeight = (GridManager.Rows * tileSize.y) + FramePaddingY;

            BoardFrameImage.rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
            BoardFrameImage.transform.position = Vector3.zero;
        }
        UpdateBoardSkin();
    }

    private void UpdateBoardSkin()
    {
        string equippedSkinID = PlayerPrefs.GetString("Equipped_Skin", "skin_default");

        if (AchievementManager.Instance != null && AchievementManager.Instance.Database != null)
        {
            foreach (var data in AchievementManager.Instance.Database.Achievements)
            {
                if (data.RewardItem is SkinData skinData && skinData.ItemID == equippedSkinID)
                {
                    foreach (var sr in LightTiles) sr.sprite = skinData.LightTileSprite;
                    foreach (var sr in DarkTiles) sr.sprite = skinData.DarkTileSprite;

                    if (BoardFrameImage != null)
                    {
                        if (skinData.FrameSprite != null)
                        {
                            BoardFrameImage.sprite = skinData.FrameSprite;
                        }
                        else
                        {
                            BoardFrameImage.sprite = DefaultFrameSprite;
                        }

                        BoardFrameImage.type = Image.Type.Tiled;
                        BoardFrameImage.pixelsPerUnitMultiplier = 24f;
                        BoardFrameImage.color = skinData.FrameColor;
                    }
                    break;
                }
            }
        }
    }
}