using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // [추가] UI 컴포넌트 사용을 위해 필수

public class BoardRenderer : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject BaseTilePrefab;

    [Header("Board Frame Settings")]
    public Image BoardFrameImage; // [수정] SpriteRenderer -> Image로 변경
    public float FramePaddingX = 1.0f;
    public float FramePaddingY = 1.0f;

    private List<SpriteRenderer> lightTiles = new List<SpriteRenderer>();
    private List<SpriteRenderer> darkTiles = new List<SpriteRenderer>();
    private Sprite defaultFrameSprite;

    private void Awake()
    {
        // 시작할 때 원래 적용되어 있던 기본 이미지를 저장해둡니다.
        if (BoardFrameImage != null)
        {
            defaultFrameSprite = BoardFrameImage.sprite;
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

        lightTiles.Clear();
        darkTiles.Clear();

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

                if ((Y + X) % 2 == 0) lightTiles.Add(sr);
                else darkTiles.Add(sr);
            }
        }

        if (BoardFrameImage != null)
        {
            float totalWidth = (GridManager.Columns * tileSize.x) + FramePaddingX;
            float totalHeight = (GridManager.Rows * tileSize.y) + FramePaddingY;

            // [수정] Image 컴포넌트는 size 대신 rectTransform.sizeDelta를 사용합니다.
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
                    foreach (var sr in lightTiles) sr.sprite = skinData.LightTileSprite;
                    foreach (var sr in darkTiles) sr.sprite = skinData.DarkTileSprite;

                    if (BoardFrameImage != null)
                    {
                        if (skinData.FrameSprite != null)
                        {
                            BoardFrameImage.sprite = skinData.FrameSprite;
                        }
                        else
                        {
                            BoardFrameImage.sprite = defaultFrameSprite;
                        }

                        // [추가] 개발자님이 원하시던 설정 적용!
                        BoardFrameImage.type = Image.Type.Sliced;
                        BoardFrameImage.pixelsPerUnitMultiplier = 44f;
                        BoardFrameImage.color = skinData.FrameColor;
                    }
                    break;
                }
            }
        }
    }
}