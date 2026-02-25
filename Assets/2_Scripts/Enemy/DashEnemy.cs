using UnityEngine;

public class DashEnemy : EnemyBase
{
    public int DashAmount = 5;

    public override void CalculateNextPosition()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
        if (Player == null) return;

        int DiffX = Player.CurrentX - CurrentX;
        int DiffY = Player.CurrentY - CurrentY;

        int MoveDirX = 0;
        int MoveDirY = 0;

        if (Mathf.Abs(DiffX) > Mathf.Abs(DiffY))
        {
            MoveDirX = (int)Mathf.Sign(DiffX);
        }
        else if (Mathf.Abs(DiffY) > Mathf.Abs(DiffX))
        {
            MoveDirY = (int)Mathf.Sign(DiffY);
        }
        else
        {
            if (Random.value > 0.5f) MoveDirX = (int)Mathf.Sign(DiffX);
            else MoveDirY = (int)Mathf.Sign(DiffY);
        }

        for (int I = 0; I < DashAmount; I++)
        {
            int NextX = CurrentX + MoveDirX;
            int NextY = CurrentY + MoveDirY;

            if (!GridManager.IsValidGridPosition(NextX, NextY)) break;

            if (IsPositionOccupied(NextX, NextY)) break;

            if (Player.CurrentX == NextX && Player.CurrentY == NextY)
            {
                CurrentX = NextX;
                CurrentY = NextY;
                break;
            }

            CurrentX = NextX;
            CurrentY = NextY;
        }
    }
}
