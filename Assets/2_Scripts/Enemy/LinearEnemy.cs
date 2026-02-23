using UnityEngine;

public class LinearEnemy : EnemyBase
{
    public override void CalculateNextPosition()
    {
        CurrentY -= 1;
    }
}
