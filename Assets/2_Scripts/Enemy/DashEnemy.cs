using System.Collections.Generic;
using UnityEngine;

public class DashEnemy : EnemyBase
{
    private static readonly Vector2Int[] Orthogonals = new Vector2Int[]
    {
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0),
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1)
    };

    public override void CalculateNextPosition()
    {
        if (Player == null) return;

        Vector2Int bestMove = new Vector2Int(CurrentX, CurrentY);
        // [수정] float.MaxValue로 초기화 (기존: 현재 거리 → 인접 시 이동 안 하는 버그)
        float minDistance = float.MaxValue;

        foreach (Vector2Int dir in Orthogonals)
        {
            int checkX = CurrentX;
            int checkY = CurrentY;

            // [수정] for(DashAmount) → while(true) : 보드 끝까지 이동
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
