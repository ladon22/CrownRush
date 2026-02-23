using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject TilePrefab;

    public Color ColorA = Color.white;
    public Color ColorB = Color.grey;
    public Vector2 GetTileSize()
    {
        SpriteRenderer SpriteRenderer = TilePrefab.GetComponent<SpriteRenderer>();
        if (TilePrefab == null || SpriteRenderer == null) return Vector2.one;
        return SpriteRenderer.bounds.size;
    }

    public void GenerateBoardVisuals(GridManager GridManager)
    {
        if (TilePrefab == null || GridManager == null) return;
        for (int Y = 0; Y < GridManager.Rows; Y++)
        {
            for (int X = 0; X < GridManager.Columns; X++)
            {
                Vector2 TilePosition = GridManager.GetWorldPosition(X, Y);
                GameObject NewTile = Instantiate(TilePrefab, TilePosition, Quaternion.identity);
                NewTile.transform.SetParent(this.transform);
                NewTile.name = $"Tile_{Y}_{X}";

                SpriteRenderer SpriteRenderer = NewTile.GetComponent<SpriteRenderer>();
                if (SpriteRenderer != null) 
                {
                    SpriteRenderer.color = (Y + X) % 2 == 0 ? ColorA : ColorB;
                }
                
            }
        }
    }
}
