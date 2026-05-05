using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject BaseTilePrefab;

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
                    break;
                }
            }
        }
    }
}
