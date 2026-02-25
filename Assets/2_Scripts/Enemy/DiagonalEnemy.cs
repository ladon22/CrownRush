using UnityEngine;

public class DiagonalEnemy : EnemyBase
{
    public int MoveAmount = 3;

    public override void CalculateNextPosition()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
        if (Player == null) return;

        int DiffX = Player.CurrentX - CurrentX;
        int DiffY = Player.CurrentY - CurrentY;

        int MoveDirX = DiffX == 0 ? (Random.value > 0.5f ? 1 : -1) : (int)Mathf.Sign(DiffX);
        int MoveDirY = DiffY == 0 ? (Random.value > 0.5f ? 1 : -1) : (int)Mathf.Sign(DiffY);

        for (int i = 0; i < MoveAmount; i++)
        {
            int NextX = CurrentX + MoveDirX;
            int NextY = CurrentY + MoveDirY;

            if (!GridManager.IsValidGridPosition(NextX, NextY))
            {
                break;
            }

            if (IsPositionOccupied(NextX, NextY))
            {
                break;
            }

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
