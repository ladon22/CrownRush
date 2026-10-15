using UnityEngine;

public abstract class SlidingPieceEnemy : EnemyBase
{
    protected abstract Vector2Int[] MoveVectors { get; }

    public override void CalculateNextPosition()
    {
        if (Player == null) return;
        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = float.MaxValue;
        foreach (Vector2Int Dir in MoveVectors)
        {
            int CheckX = CurrentX;
            int CheckY = CurrentY;
            while (true)
            {
                CheckX += Dir.x;
                CheckY += Dir.y;
                if (!GridManager.IsValidGridPosition(CheckX, CheckY)) break;
                if (Player.CurrentX == CheckX && Player.CurrentY == CheckY)
                {
                    BestMove = new Vector2Int(CheckX, CheckY);
                    MinDistance = 0;
                    break;
                }
                if (IsPositionOccupied(CheckX, CheckY)) break;
                float dist = Vector2.Distance(new Vector2(CheckX, CheckY), 
                    new Vector2(Player.CurrentX, Player.CurrentY));
                if (dist < MinDistance)
                {
                    MinDistance = dist;
                    BestMove = new Vector2Int(CheckX, CheckY);
                }
            }
            if (MinDistance == 0) break;
        }
        CurrentX = BestMove.x;
        CurrentY = BestMove.y;
    }
}
