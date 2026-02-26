using System.Collections.Generic;
using UnityEngine;

public class LinearEnemy : EnemyBase
{
    public override void CalculateNextPosition()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
        if (Player == null) return;

        int DiffX = Player.CurrentX - CurrentX;
        int DiffY = Player.CurrentY - CurrentY;

        if (Mathf.Abs(DiffX) > Mathf.Abs(DiffY))
        {
            CurrentX += (int)Mathf.Sign(DiffX);
        }
        else if (DiffY != 0)
        {
            CurrentY += (int)Mathf.Sign(DiffY);
        }
        else
        {
            CurrentX += (int)Mathf.Sign(DiffX);
        }
    }
}
