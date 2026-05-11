using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject BaseTilePrefab;

    [Header("Board Frame Settings")]
    public SpriteRenderer BoardFrameRenderer; // 씬에 만들어둔 9-Slice 테두리 오브젝트
    public float FramePaddingX = 1.0f; // 좌우 테두리 여백 (두께에 맞게 조절)
    public float FramePaddingY = 1.0f; // 상하 테두리 여백

    private List<SpriteRenderer> lightTiles = new List<SpriteRenderer>();
    private List<SpriteRenderer> darkTiles = new List<SpriteRenderer>();

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

                // 좌표에 따라 밝은 칸인지 어두운 칸인지 기억
                if ((Y + X) % 2 == 0) lightTiles.Add(sr);
                else darkTiles.Add(sr);
            }
        }

        if (BoardFrameRenderer != null)
        {
            // 1. 체스판 전체의 가로/세로 길이를 계산합니다.
            float totalWidth = (GridManager.Columns * tileSize.x) + FramePaddingX;
            float totalHeight = (GridManager.Rows * tileSize.y) + FramePaddingY;

            // 2. 9-Slice Sprite의 모서리를 유지한 채로 크기를 변경합니다.
            BoardFrameRenderer.size = new Vector2(totalWidth, totalHeight);

            // 3. GridManager 로직상 보드의 중심이 (0,0)이므로 테두리도 (0,0)으로 이동시킵니다.
            BoardFrameRenderer.transform.position = Vector3.zero;
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
                    // 방송이 울리면 저장된 리스트의 이미지를 순식간에 교체!
                    foreach (var sr in lightTiles) sr.sprite = skinData.LightTileSprite;
                    foreach (var sr in darkTiles) sr.sprite = skinData.DarkTileSprite;
                    if (BoardFrameRenderer != null)
                    {
                        BoardFrameRenderer.color = skinData.FrameColor;
                    }
                    break;
                }
            }
        }
    }
}
