using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int Columns = 5;
    public int Rows = 7;
    public Vector2 TileSize;
    private Vector2[,] GridPositions;

    public void InitializeGrid(Vector2 PrefabSize)
    {
        TileSize = PrefabSize;
        GridPositions = new Vector2[Rows, Columns];
        float StartOffsetX = -(Columns - 1) * TileSize.x / 2f;
        float StartOffsetY = -(Rows - 1) * TileSize.y / 2f;

        for (int Y = 0; Y < Rows; Y++)
        {
            for (int X = 0; X < Columns; X++)
            {
                float PosX = StartOffsetX + X * TileSize.x;
                float PosY = StartOffsetY + Y * TileSize.y;
                GridPositions[Y, X] = new Vector2(PosX, PosY);
            }
        }
    }

    public Vector2 GetWorldPosition(int X, int Y)
    {
        if (IsValidGridPosition(X, Y))
        {
            return GridPositions[Y, X];
        }

        Debug.LogWarning("유효하지 않은 그리드 좌표를 요청했습니다.");
        return Vector2.zero;
    }

    public bool IsValidGridPosition(int X, int Y)
    {
        return X >= 0 && X < Columns && Y >= 0 && Y < Rows;
    }

    public void GetPlayerStartIndex(out int StartX, out int StartY)
    {
        StartX = Columns / 2;
        StartY = 0;
    }
}
