using UnityEngine;

public class LinearEnemy : EnemyBase
{
    public override void CalculateNextPosition()
    {
        if (!IsPositionOccupied(CurrentX, CurrentY - 1))
        {
            CurrentY -= 1;
        }
    }
}
