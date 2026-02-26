using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemy : EnemyBase
{
    private Vector2Int[] KnightMoves = new Vector2Int[]
        {
            new Vector2Int(1, 2), new Vector2Int(1, -2),
            new Vector2Int(-1, 2), new Vector2Int(-1, -2),
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1)
        };

    public override void CalculateNextPosition()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
        if (Player == null) return;

        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = float.MaxValue;
        bool FoundValidMove = false;

        foreach (Vector2Int Move in KnightMoves)
        {
            int NextX = CurrentX + Move.x;
            int NextY = CurrentY + Move.y;

            if (!GridManager.IsValidGridPosition(NextX, NextY)) continue;
            if (IsPositionOccupied(NextX, NextY)) continue;

            float Distance = Vector2.Distance(new Vector2(NextX, NextY), new Vector2(Player.CurrentX, Player.CurrentY));

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

    //protected override IEnumerator AnimateMove(int StartX, int StartY, int TargetX, int TargetY)
    //{
    //    int DiffX = Mathf.Abs(TargetX - StartX);
    //    int DiffY = Mathf.Abs(TargetY - StartY);

    //    Vector2 Waypoint;
    //    if (DiffY == 2) Waypoint = GridManager.GetWorldPosition(StartX, TargetY);
    //    else Waypoint = GridManager.GetWorldPosition(TargetX, StartY);            

    //    while ((Vector2)transform.position != Waypoint)
    //    {
    //        transform.position = Vector2.MoveTowards(transform.position, Waypoint, MoveSpeed * Time.deltaTime);
    //        yield return null;
    //    }

    //    Vector2 FinalPosition = GridManager.GetWorldPosition(TargetX, TargetY);
    //    while ((Vector2)transform.position != FinalPosition)
    //    {
    //        transform.position = Vector2.MoveTowards(transform.position, FinalPosition, MoveSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //}
}
