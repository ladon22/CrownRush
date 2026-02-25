using UnityEngine;

public class DiagonalEnemy : EnemyBase
{
    public int MoveAmount = 2;
    private int XDirection = 1;

    public override void Initialize(EnemyManager EnemyManager, GridManager GridManager, int StartX, int StartY)
    {
        base.Initialize(EnemyManager, GridManager, StartX, StartY);
        XDirection = Random.value > 0.5 ? 1 : -1;
        if (CurrentX == 0)
        {
            XDirection = 1;
        }
        else if (CurrentX == GridManager.Columns - 1)
        {
            XDirection = -1;
        }
    }
    public override void CalculateNextPosition()
    {
        if ((CurrentX <= 0 && XDirection == -1) || (CurrentX >= GridManager.Columns - 1 && XDirection == 1))
        {
            XDirection *= -1;
        }
        for (int i = 0; i < MoveAmount; i++)
        {
            int NextX = CurrentX + XDirection;
            int NextY = CurrentY - 1;

            if (NextX < 0 || NextX >= GridManager.Columns)
            {
                XDirection *= -1;
                break;
            }

            if (IsPositionOccupied(NextX, NextY))
            {
                break;
            }

            CurrentX = NextX;
            CurrentY = NextY;

            if (CurrentY == 0)
            {
                break;
            }
        }
    }
}
