using UnityEngine;

public class QueenEnemy : EnemyBase
{
    private Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public override void CalculateNextPosition()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
        if (Player == null) return;

        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = float.MaxValue;

        // 8방향으로 각각 레이저를 쏘듯이 탐색합니다.
        foreach (Vector2Int Dir in Directions)
        {
            int CheckX = CurrentX;
            int CheckY = CurrentY;

            while (true)
            {
                CheckX += Dir.x;
                CheckY += Dir.y;

                // 1. 맵 밖으로 나가면 탐색 종료 (다음 방향으로)
                if (!GridManager.IsValidGridPosition(CheckX, CheckY)) break;

                // 2. 누군가 자리를 차지하고 있다면?
                if (IsPositionOccupied(CheckX, CheckY))
                {
                    // 만약 그 자리에 있는 게 플레이어라면? 당장 돌진해서 공격해야 합니다!
                    if (Player.CurrentX == CheckX && Player.CurrentY == CheckY)
                    {
                        BestMove = new Vector2Int(CheckX, CheckY);
                        MinDistance = 0; // 최우선 순위
                        break;
                    }
                    else
                    {
                        // 다른 적이나 아이템에 막혔으므로 이 방향 탐색은 여기까지.
                        break;
                    }
                }

                // 3. 빈 공간이라면 플레이어와의 거리를 잽니다.
                float Dist = Vector2.Distance(new Vector2(CheckX, CheckY), new Vector2(Player.CurrentX, Player.CurrentY));

                // 지금까지 찾은 칸들보다 플레이어와 더 가깝다면 이 칸을 최적의 이동 위치로 저장합니다.
                if (Dist < MinDistance)
                {
                    MinDistance = Dist;
                    BestMove = new Vector2Int(CheckX, CheckY);
                }
            }

            // 이미 플레이어를 공격할 수 있는 완벽한 수를 찾았다면 더 탐색할 필요가 없습니다.
            if (MinDistance == 0) break;
        }

        // 최종적으로 결정된 최적의 위치로 이동합니다.
        CurrentX = BestMove.x;
        CurrentY = BestMove.y;
    }
}
