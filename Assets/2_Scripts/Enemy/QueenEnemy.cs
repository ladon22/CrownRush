using UnityEngine;

public class QueenEnemy : EnemyBase
{
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public override void CalculateNextPosition()
    {
        if (Player == null) return;

        Vector2Int BestMove = new Vector2Int(CurrentX, CurrentY);
        float MinDistance = float.MaxValue;

        // 8�������� ���� �������� ����� Ž���մϴ�.
        foreach (Vector2Int Dir in Directions)
        {
            int CheckX = CurrentX;
            int CheckY = CurrentY;

            while (true)
            {
                CheckX += Dir.x;
                CheckY += Dir.y;

                // 1. �� ������ ������ Ž�� ���� (���� ��������)
                if (!GridManager.IsValidGridPosition(CheckX, CheckY)) break;

                // 2. ������ �ڸ��� �����ϰ� �ִٸ�?
                if (IsPositionOccupied(CheckX, CheckY))
                {
                    // ���� �� �ڸ��� �ִ� �� �÷��̾���? ���� �����ؼ� �����ؾ� �մϴ�!
                    if (Player.CurrentX == CheckX && Player.CurrentY == CheckY)
                    {
                        BestMove = new Vector2Int(CheckX, CheckY);
                        MinDistance = 0; // �ֿ켱 ����
                        break;
                    }
                    else
                    {
                        // �ٸ� ���̳� �����ۿ� �������Ƿ� �� ���� Ž���� �������.
                        break;
                    }
                }

                // 3. �� �����̶�� �÷��̾���� �Ÿ��� ��ϴ�.
                float Dist = Vector2.Distance(new Vector2(CheckX, CheckY), new Vector2(Player.CurrentX, Player.CurrentY));

                // ���ݱ��� ã�� ĭ�麸�� �÷��̾�� �� �����ٸ� �� ĭ�� ������ �̵� ��ġ�� �����մϴ�.
                if (Dist < MinDistance)
                {
                    MinDistance = Dist;
                    BestMove = new Vector2Int(CheckX, CheckY);
                }
            }

            // �̹� �÷��̾ ������ �� �ִ� �Ϻ��� ���� ã�Ҵٸ� �� Ž���� �ʿ䰡 �����ϴ�.
            if (MinDistance == 0) break;
        }

        // ���������� ������ ������ ��ġ�� �̵��մϴ�.
        CurrentX = BestMove.x;
        CurrentY = BestMove.y;
    }
}
