using UnityEngine;

public class LinearEnemy : EnemyBase
{
    bool bIsResting = false;

    public override void CalculateNextPosition()
    {
        if (bIsResting)
        {
            bIsResting = false;
            return;
        }
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

        bIsResting = true;
    }
}
