using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemy : EnemyBase
{
    private static readonly Vector2Int[] MoveVectors = new Vector2Int[]
    {
        new Vector2Int( 1,  2), new Vector2Int( 1, -2),
        new Vector2Int(-1,  2), new Vector2Int(-1, -2),
        new Vector2Int( 2,  1), new Vector2Int( 2, -1),
        new Vector2Int(-2,  1), new Vector2Int(-2, -1)
    };

    public override void CalculateNextPosition()
    {
        if (Player == null) return;
        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = float.MaxValue;
        bool FoundValidMove = false;

        foreach (Vector2Int Move in MoveVectors)
        {
            int NextX = CurrentX + Move.x;
            int NextY = CurrentY + Move.y;

            if (!GridManager.IsValidGridPosition(NextX, NextY)) continue;
            if (IsPositionOccupied(NextX, NextY)) continue;

            float Distance = Vector2.Distance(new Vector2(NextX, NextY),
                new Vector2(Player.CurrentX, Player.CurrentY));

            if (Distance < MinDistance)
            {
                MinDistance = Distance;
                BestMove = new Vector2Int(NextX, NextY);
                FoundValidMove = true;
            }
        }
        if (FoundValidMove)
        {
            CurrentX = BestMove.x;
            CurrentY = BestMove.y;
        }
    }
}
