using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GridManager GridManager;
    public GameObject[] EnemyPrefabs;

    [Header("Spawn Settings")]
    public int MaxEnemyCount = 4;

    [Header("Enemy Bag Settings")]
    public int BasicEnemyCount = 5;   
    public int DiagonalEnemyCount = 2;
    public int DashEnemyCount = 1;
    public int KnightEnemyCount = 1;

    private List<int> EnemyBag = new List<int>();

    [HideInInspector]
    public List<EnemyBase> ActiveEnemies = new List<EnemyBase>();

    private PlayerController _player;
    private PlayerController CachedPlayer => _player != null ? _player : _player = FindFirstObjectByType<PlayerController>();
    private ItemManager _itemManager;
    private ItemManager CachedItemManager => _itemManager != null ? _itemManager : _itemManager = FindFirstObjectByType<ItemManager>();

    public void SpawnEnemy()
    {
        MaintainEnemyCount();
    }

    private void MaintainEnemyCount()
    {
        if (EnemyPrefabs.Length == 0 || GridManager == null) return;

        ActiveEnemies.RemoveAll(Enemy => Enemy == null || !Enemy.gameObject.activeInHierarchy);

        int SpawnAmount = MaxEnemyCount - ActiveEnemies.Count;

        if (SpawnAmount <= 0) return;

        for (int i = 0; i < SpawnAmount; i++)
        {
            if (EnemyBag.Count == 0)
            {
                RefillAndShuffleEnemyBag();
            }

            int PulledEnemyIndex = EnemyBag[0];
            GameObject PrefabToSpawn = EnemyPrefabs[PulledEnemyIndex];
            bool bIsLinearEnemy = PrefabToSpawn.GetComponent<LinearEnemy>() != null;

            int SpawnX, SpawnY;
            if (GetValidEdgeSpawnPosition(bIsLinearEnemy, out SpawnX, out SpawnY))
            {
                EnemyBag.RemoveAt(0);

                Vector2 SpawnPosition = GridManager.GetWorldPosition(SpawnX, SpawnY);
                GameObject NewEnemyObj = PoolManager.Instance.SpawnFromPool(PrefabToSpawn, SpawnPosition, Quaternion.identity);

                EnemyBase NewEnemy = NewEnemyObj.GetComponent<EnemyBase>();
                if (NewEnemy != null)
                {
                    NewEnemy.Initialize(this, GridManager, SpawnX, SpawnY);
                    ActiveEnemies.Add(NewEnemy);
                }
            }
        }

    }

    private bool GetValidEdgeSpawnPosition(bool bIsLinearEnemy, out int SpawnX, out int SpawnY)
    {
        SpawnX = -1; SpawnY = -1;
        List<Vector2Int> EdgeTiles = new List<Vector2Int>();

        for (int X = 0; X < GridManager.Columns; X++)
        {
            EdgeTiles.Add(new Vector2Int(X, 0));
            EdgeTiles.Add(new Vector2Int(X, GridManager.Rows - 1));
        }

        if (!bIsLinearEnemy)
        {
            for (int Y = 1; Y < GridManager.Rows - 1; Y++)
            {
                EdgeTiles.Add(new Vector2Int(0, Y));
                EdgeTiles.Add(new Vector2Int(GridManager.Columns - 1, Y));
            }
        }
        for (int I = 0; I < EdgeTiles.Count; I++)
        {
            int RandomIndex = Random.Range(I, EdgeTiles.Count);
            Vector2Int Temp = EdgeTiles[I];
            EdgeTiles[I] = EdgeTiles[RandomIndex];
            EdgeTiles[RandomIndex] = Temp;
        }
        foreach (Vector2Int Tile in EdgeTiles)
        {
            if (GetEnemyAtPosition(Tile.x, Tile.y) != null) continue;

            if (bIsLinearEnemy)
            {
                bool bIsColumnOccupiedByLinear = false;
                foreach (EnemyBase Enemy in ActiveEnemies)
                {
                    if (Enemy != null && Enemy.gameObject.activeInHierarchy && Enemy is LinearEnemy)
                    {
                        if (Enemy.CurrentX == Tile.x) // ���� �ٿ� �ִٸ�
                        {
                            bIsColumnOccupiedByLinear = true;
                            break;
                        }
                    }
                }
                // �̹� ���� �ٿ� ���� �ִٸ� �� ���� ��ġ�� �ǳʶݴϴ�!
                if (bIsColumnOccupiedByLinear) continue;
            }

            if (CachedPlayer != null)
            {
                int DistX = Mathf.Abs(CachedPlayer.CurrentX - Tile.x);
                int DistY = Mathf.Abs(CachedPlayer.CurrentY - Tile.y);
                if (DistX <= 1 && DistY <= 1) continue;
            }

            if (CachedItemManager != null && CachedItemManager.GetItemAtPosition(Tile.x, Tile.y) != null)
            {
                continue;
            }
            SpawnX = Tile.x;
            SpawnY = Tile.y;
            return true;
        }

        return false;
    }
    private void RefillAndShuffleEnemyBag()
    {
        EnemyBag.Clear();
        for (int i = 0; i < BasicEnemyCount; i++)
            EnemyBag.Add(0);
        for (int i = 0; i < DiagonalEnemyCount; i++)
            EnemyBag.Add(1);
        for (int i = 0; i < DashEnemyCount; i++)
            EnemyBag.Add(2);
        for (int i = 0; i < KnightEnemyCount; i++)
            EnemyBag.Add(3);

        for(int i = 0; i < EnemyBag.Count; i++)
        {
            int RandomIndex = Random.Range(i, EnemyBag.Count);
            int Temp = EnemyBag[i];
            EnemyBag[i] = EnemyBag[RandomIndex];
            EnemyBag[RandomIndex] = Temp;
        }
    }

    public IEnumerator ProcessEnemiesTurnRoutine()
    {
        ActiveEnemies.RemoveAll(Enemy => Enemy == null || !Enemy.gameObject.activeInHierarchy);

        if (CachedPlayer != null)
        {
            ActiveEnemies.Sort((A, B) =>
            {
                float DistanceA = Vector2.Distance(new Vector2(A.CurrentX, A.CurrentY), new Vector2(CachedPlayer.CurrentX, CachedPlayer.CurrentY));
                float DistanceB = Vector2.Distance(new Vector2(B.CurrentX, B.CurrentY), new Vector2(CachedPlayer.CurrentX, CachedPlayer.CurrentY));

                return DistanceA.CompareTo(DistanceB);
            });
        }
        else
        {
            ActiveEnemies.Sort((A, B) => A.CurrentY.CompareTo(B.CurrentY));
        }

        List<Coroutine> MoveCoroutines = new List<Coroutine>();

        List<EnemyBase> SnapshotEnemies = new List<EnemyBase>(ActiveEnemies);
        foreach (EnemyBase Enemy in SnapshotEnemies)
        {
            if (Enemy != null && Enemy.gameObject.activeInHierarchy)
            {
                MoveCoroutines.Add(StartCoroutine(Enemy.MoveRoutine()));
            }
        }

        foreach (Coroutine MoveRoutine in MoveCoroutines)
        {
            yield return MoveRoutine;
        }

        MaintainEnemyCount();
    }

    public EnemyBase GetEnemyAtPosition(int TargetX , int TargetY)
    {
        foreach (EnemyBase Enemy in ActiveEnemies)
        {
            if (Enemy != null && Enemy.gameObject.activeInHierarchy && Enemy.CurrentX == TargetX && Enemy.CurrentY == TargetY)
            {
                return Enemy;
            }
        }
        return null;
    }
    public void RemoveEnemy(EnemyBase EnemyToRemove)
    {
        if(ActiveEnemies.Contains(EnemyToRemove))
        {
            ActiveEnemies.Remove(EnemyToRemove);
        }
    }

    public void UpdateEnemyIntents()
    {
        foreach (EnemyBase Enemy in ActiveEnemies)
        {
            if (Enemy != null && Enemy.gameObject.activeInHierarchy)
            {
                Enemy.SetIntent(Enemy.WillMoveThisTurn());
            }
        }
    }

    public void ClearAllIntents()
    {
        foreach (EnemyBase Enemy in ActiveEnemies)
        {
            if (Enemy != null && Enemy.gameObject.activeInHierarchy)
            {
                Enemy.SetIntent(false);
            }
        }
    }
}
