using System.Collections.Generic;
using UnityEngine;

public class DiagonalEnemy : EnemyBase
{
    private static readonly Vector2Int[] Diagonals = new Vector2Int[]
    {
        new Vector2Int( 1,  1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  1),
        new Vector2Int(-1, -1)
    };

    public override void CalculateNextPosition()
    {
        if (Player == null) return;

        Vector2Int bestMove = new Vector2Int(CurrentX, CurrentY);
        float minDistance = float.MaxValue;

        foreach (Vector2Int dir in Diagonals)
        {
            int checkX = CurrentX;
            int checkY = CurrentY;

            // [수정] for(MoveAmount) → while(true) : 보드 끝까지 이동
            while (true)
            {
                checkX += dir.x;
                checkY += dir.y;

                if (!GridManager.IsValidGridPosition(checkX, checkY)) break;

                if (IsPositionOccupied(checkX, checkY))
                {
                    if (Player.CurrentX == checkX && Player.CurrentY == checkY)
                    {
                        bestMove = new Vector2Int(checkX, checkY);
                        minDistance = 0;
                    }
                    break;
                }

                float dist = Vector2.Distance(
                    new Vector2(checkX, checkY),
                    new Vector2(Player.CurrentX, Player.CurrentY));

                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestMove = new Vector2Int(checkX, checkY);
                }
            }

            if (minDistance == 0) break;
        }

        CurrentX = bestMove.x;
        CurrentY = bestMove.y;
    }
}
