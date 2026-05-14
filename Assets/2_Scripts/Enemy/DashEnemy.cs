using System.Collections.Generic;
using UnityEngine;

public class DashEnemy : EnemyBase
{
    public int DashAmount = 3;

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

        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = Vector2.Distance(new Vector2(CurrentX, CurrentY), new Vector2(Player.CurrentX, Player.CurrentY));
        
        int CheckX = CurrentX;
        int CheckY = CurrentY;

        for (int I = 0; I < DashAmount; I++)
        {
            CheckX += MoveDirX;
            CheckY += MoveDirY;

            if (!GridManager.IsValidGridPosition(CheckX, CheckY)) break;

            if (IsPositionOccupied(CheckX, CheckY))
            {
                // 플레이어를 만나면 무조건 공격해야 하므로 거리 0으로 세팅 후 탐색 종료
                if (Player.CurrentX == CheckX && Player.CurrentY == CheckY)
                {
                    BestMove = new Vector2Int(CheckX, CheckY);
                    MinDistance = 0;
                }
                break;
            }

            // 핵심: 지나가면서 플레이어와의 거리를 재보고, 더 가깝다면 그 자리를 최종 목적지로 갱신합니다.
            float Dist = Vector2.Distance(new Vector2(CheckX, CheckY), new Vector2(Player.CurrentX, Player.CurrentY));
            if (Dist < MinDistance)
            {
                MinDistance = Dist;
                BestMove = new Vector2Int(CheckX, CheckY);
            }
        }

        // 최종적으로 플레이어와 가장 가까웠던 타일로 이동
        CurrentX = BestMove.x;
        CurrentY = BestMove.y;
    }
}
