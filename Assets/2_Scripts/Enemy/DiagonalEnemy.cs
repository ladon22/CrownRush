using System.Collections.Generic;
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

        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = Vector2.Distance(new Vector2(CurrentX, CurrentY), new Vector2(Player.CurrentX, Player.CurrentY));

        int CheckX = CurrentX;
        int CheckY = CurrentY;

        for (int i = 0; i < MoveAmount; i++)
        {
            CheckX += MoveDirX;
            CheckY += MoveDirY;

            if (!GridManager.IsValidGridPosition(CheckX, CheckY)) break;

            if (IsPositionOccupied(CheckX, CheckY))
            {
                if (Player.CurrentX == CheckX && Player.CurrentY == CheckY)
                {
                    BestMove = new Vector2Int(CheckX, CheckY);
                    MinDistance = 0;
                }
                break;
            }

            // 핵심: 대각선으로 움직이다가 플레이어를 스쳐 멀어지기 전에 멈춥니다.
            float Dist = Vector2.Distance(new Vector2(CheckX, CheckY), new Vector2(Player.CurrentX, Player.CurrentY));
            if (Dist < MinDistance)
            {
                MinDistance = Dist;
                BestMove = new Vector2Int(CheckX, CheckY);
            }
        }

        CurrentX = BestMove.x;
        CurrentY = BestMove.y;
    }
}
