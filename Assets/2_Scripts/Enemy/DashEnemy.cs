using UnityEngine;

public class DashEnemy : EnemyBase
{
    private bool IsResting = false;
    public int DashAmount = 3;

    public override void CalculateNextPosition()
    {
        if (IsResting)
        {
            IsResting = false;
            return;
        }

        for (int i = 0; i < DashAmount; i++)
        {
            if (IsPositionOccupied(CurrentX, CurrentY - 1))
            {
                break;
            }
            CurrentY -= 1;
            if (CurrentY <= 0)
            {
                break;
            }
        }
        IsResting = true;
    }
}
