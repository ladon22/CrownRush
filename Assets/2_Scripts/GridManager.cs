using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance {  get; private set; }

    [Header("Grid Config")]
    [SerializeField] private int Width = 5;
    [SerializeField] private int Height = 8;

    [Header("References")]
    [SerializeField] private RectTransform GridContainer;
    [SerializeField] private GameObject TilePrefab;

    [Header("Visuals")]
    [SerializeField] private Color ColorA = Color.white;
    [SerializeField] private Color ColorB = Color.gray;
    public float CellSize { get; private set; }
   

    private int[,] GridArray;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        GridArray = new int[Height, Width];
        CellSize = GridContainer.rect.width / Width;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                GridArray[y, x] = 0;
                GameObject NewTile = Instantiate(TilePrefab, GridContainer);
                NewTile.name = $"Tile_{y}_{x}";
                
                RectTransform Rect = NewTile.GetComponent<RectTransform>();
                Rect.anchoredPosition = GetLocalPosition(x, y);
                Image TileImage = NewTile.GetComponent<Image>();
                if (TileImage != null)
                    TileImage.color = (y+x) % 2 == 0 ? ColorA : ColorB;
            }
        }
    }

    public Vector2 GetLocalPosition(int X, int Y)
    {
        float PosX = (X * CellSize) + (CellSize * 0.5f);
        float PosY = ((Height - 1 - Y) * CellSize) + (CellSize * 0.5f);
        return new Vector2(PosX, PosY);
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public void SetGridValue(int X, int Y, int Value)
    {
        if (IsValidPosition(X, Y))
            GridArray[Y, X] = Value;
    }

    public int GetGridValue(int X, int Y)
    {
        if (IsValidPosition(X, Y))
            return GridArray[Y, X];
        return -1;
    }
}
