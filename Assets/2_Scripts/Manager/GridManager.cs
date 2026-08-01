using UnityEngine;
using UnityEngine.UI;

public interface IBoardOccupant
{
    int CurrentX { get; }
    int CurrentY { get; }
}

public class GridManager : MonoBehaviour
{
    public int Columns = 5;
    public int Rows = 7;
    public Vector2 TileSize;
    private Vector2[,] GridPositions;
    private IBoardOccupant[,] Occupants;

    public void InitializeGrid(Vector2 PrefabSize)
    {
        TileSize = PrefabSize;
        GridPositions = new Vector2[Rows, Columns];
        Occupants = new IBoardOccupant[Rows, Columns];
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

    // 좌표 X,Y에 Occupant를 등록합니다. 이미 다른 점유자가 있어도 덮어씁니다
    // (승진 등 같은 칸에 새 오브젝트가 들어서는 경우를 위함).
    public void PlaceOccupant(IBoardOccupant Occupant, int X, int Y)
    {
        if (!IsValidGridPosition(X, Y)) return;
        Occupants[Y, X] = Occupant;
    }

    // Occupant가 여전히 해당 칸을 점유하고 있을 때만 제거합니다.
    // (이미 다른 오브젝트가 그 칸을 차지했다면 잘못 지우지 않도록 방지)
    public void RemoveOccupant(IBoardOccupant Occupant, int X, int Y)
    {
        if (!IsValidGridPosition(X, Y)) return;
        if (ReferenceEquals(Occupants[Y, X], Occupant))
        {
            Occupants[Y, X] = null;
        }
    }

    public void MoveOccupant(IBoardOccupant Occupant, int FromX, int FromY, int ToX, int ToY)
    {
        RemoveOccupant(Occupant, FromX, FromY);
        PlaceOccupant(Occupant, ToX, ToY);
    }

    public IBoardOccupant GetOccupant(int X, int Y)
    {
        if (!IsValidGridPosition(X, Y)) return null;
        return Occupants[Y, X];
    }

    public bool IsOccupied(int X, int Y)
    {
        return GetOccupant(X, Y) != null;
    }

    public Vector2 GetWorldPosition(int X, int Y)
    {
        if (IsValidGridPosition(X, Y))
        {
            return GridPositions[Y, X];
        }

        return Vector2.zero;
    }

    public bool IsValidGridPosition(int X, int Y)
    {
        return X >= 0 && X < Columns && Y >= 0 && Y < Rows;
    }

    public void GetPlayerStartIndex(out int StartX, out int StartY)
    {
        StartX = Columns / 2;
        StartY = Rows / 2;
    }
}
