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


        if (Mathf.Abs(DiffX) == 1 && Mathf.Abs(DiffY) == 1)
        {
            CurrentX += DiffX;
            CurrentY += DiffY;
            return;
        }

        int NextX = CurrentX;
        int NextY = CurrentY;

        if (Mathf.Abs(DiffX) > Mathf.Abs(DiffY))
        {
            NextX += (int)Mathf.Sign(DiffX);
        }
        else if (DiffY != 0)
        {
            NextY += (int)Mathf.Sign(DiffY);
        }
        else if (DiffX != 0)
        {
            NextX += (int)Mathf.Sign(DiffX);
        }

        if (NextX == Player.CurrentX && NextY == Player.CurrentY)
        {
            // 막혔을 때, 다른 방향으로 우회를 시도합니다.
            if (NextX != CurrentX && DiffY != 0)
            {
                NextX = CurrentX;
                NextY = CurrentY + (int)Mathf.Sign(DiffY);
            }
            else if (NextY != CurrentY && DiffX != 0)
            {
                NextX = CurrentX + (int)Mathf.Sign(DiffX);
                NextY = CurrentY;
            }
            else
            {
                NextX = CurrentX;
                NextY = CurrentY;
            }
        }

        CurrentX = NextX;
        CurrentY = NextY;
    }
}
