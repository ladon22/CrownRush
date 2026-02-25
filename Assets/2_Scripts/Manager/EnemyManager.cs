using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GridManager GridManager;
    public GameObject[] EnemyPrefabs;

    [Header("Spawn Settings")]
    public int SpawnInterval = 3;
    private int CurrentTurnCount = 0;

    [Header("Bag Settings")]
    public int BasicEnemyCount = 5;   
    public int DiagonalEnemyCount = 2;
    public int DashEnemyCount = 1;
    public int KnightEnemyCount = 1;
    public int EmptySpaceCount = 6;

    private List<int> SpawnBag = new List<int>();

    [HideInInspector]
    public List<EnemyBase> ActiveEnemies = new List<EnemyBase>();

    public void SpawnEnemy()
    {
        if (EnemyPrefabs.Length == 0 || GridManager == null) return;

        //int RandomX = Random.Range(0, GridManager.Columns);
        int SpawnY = GridManager.Rows - 1;

        for (int i = 0; i < GridManager.Columns; i++)
        {
            if (SpawnBag.Count == 0)
            {
                RefillAndShuffleBag();
            }

            int PulledIndex = SpawnBag[0];
            SpawnBag.RemoveAt(0);

            if (PulledIndex == -1) continue;

            GameObject PrefabToSpawn = EnemyPrefabs[PulledIndex];
            Vector2 SpawnPosition = GridManager.GetWorldPosition(i, SpawnY);
            GameObject NewEnemyObj = Instantiate(PrefabToSpawn, SpawnPosition, Quaternion.identity);

            EnemyBase NewEnemy = NewEnemyObj.GetComponent<EnemyBase>();
            if (NewEnemy != null)
            {
                NewEnemy.Initialize(this, GridManager, i, SpawnY);
                ActiveEnemies.Add(NewEnemy);
            }
        }

        //int RandomEnemyIndex = Random.Range(0, EnemyPrefabs.Length);
        //GameObject PrefabToSpawn = EnemyPrefabs[RandomEnemyIndex];

        //Vector2 SpawnPosition = GridManager.GetWorldPosition(RandomX, SpawnY);
        //GameObject NewEnemyObj = Instantiate(PrefabToSpawn, SpawnPosition, Quaternion.identity);

        //EnemyBase NewEnemy = NewEnemyObj.GetComponent<EnemyBase>();
        //if (NewEnemy != null)
        //{
        //    NewEnemy.Initialize(this, GridManager, RandomX, SpawnY);
        //    ActiveEnemies.Add(NewEnemy);
        //}
    }

    private void RefillAndShuffleBag()
    {
        SpawnBag.Clear();
        for (int i = 0; i < BasicEnemyCount; i++)
            SpawnBag.Add(0);
        for (int i = 0; i < DiagonalEnemyCount; i++)
            SpawnBag.Add(1);
        for (int i = 0; i < DashEnemyCount; i++)
            SpawnBag.Add(2);
        for (int i = 0; i < KnightEnemyCount; i++)
            SpawnBag.Add(3);

        for (int i = 0; i < EmptySpaceCount; i++)
            SpawnBag.Add(-1);

        for(int i = 0; i < SpawnBag.Count; i++)
        {
            int RandomIndex = Random.Range(i, SpawnBag.Count);
            int Temp = SpawnBag[i];
            SpawnBag[i] = SpawnBag[RandomIndex];
            SpawnBag[RandomIndex] = Temp;
        }
    }

    public IEnumerator ProcessEnemiesTurnRoutine()
    {
        ActiveEnemies.RemoveAll(Enemy => Enemy == null);
        ActiveEnemies.Sort((A, B) => A.CurrentY.CompareTo(B.CurrentY));

        List<Coroutine> MoveCoroutines = new List<Coroutine>();

        List<EnemyBase> SnapshotEnemies = new List<EnemyBase>(ActiveEnemies);
        foreach (EnemyBase Enemy in SnapshotEnemies)
        {
            if (Enemy != null)
            {
                MoveCoroutines.Add(StartCoroutine(Enemy.MoveRoutine()));
            }
        }

        foreach (Coroutine MoveRoutine in MoveCoroutines)
        {
            yield return MoveRoutine;
        }

        CurrentTurnCount++;
        if (CurrentTurnCount >= SpawnInterval)
        {
            SpawnEnemy();
            CurrentTurnCount = 0;
        }
    }

    public EnemyBase GetEnemyAtPosition(int TargetX , int TargetY)
    {
        foreach (EnemyBase Enemy in ActiveEnemies)
        {
            if (Enemy != null && Enemy.CurrentX == TargetX && Enemy.CurrentY == TargetY)
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
}
