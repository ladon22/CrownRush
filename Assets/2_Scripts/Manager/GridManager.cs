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

    public void PlaceOccupant(IBoardOccupant Occupant, int X, int Y)
    {
        if (!IsValidGridPosition(X, Y)) return;
        Occupants[Y, X] = Occupant;
    }

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
