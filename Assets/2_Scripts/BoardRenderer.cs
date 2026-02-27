using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject LightTilePrefab;
    public GameObject DarkTilePrefab;
    public Vector2 GetTileSize()
    {
        SpriteRenderer SpriteRenderer = LightTilePrefab.GetComponent<SpriteRenderer>();
        if (LightTilePrefab == null || SpriteRenderer == null) return Vector2.one;
        return SpriteRenderer.bounds.size;
    }

    public void GenerateBoardVisuals(GridManager GridManager)
    {
        if (LightTilePrefab == null || GridManager == null) return;
        for (int Y = 0; Y < GridManager.Rows; Y++)
        {
            for (int X = 0; X < GridManager.Columns; X++)
            {
                Vector2 TilePosition = GridManager.GetWorldPosition(X, Y);
                GameObject NewTile = (Y + X) % 2 == 0 ? Instantiate(LightTilePrefab, TilePosition, Quaternion.identity) : Instantiate(DarkTilePrefab, TilePosition, Quaternion.identity);
                NewTile.transform.SetParent(this.transform);
                NewTile.name = $"Tile_{Y}_{X}";
            }
        }
    }
}
