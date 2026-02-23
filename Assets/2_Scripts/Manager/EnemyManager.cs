using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GridManager GridManager;
    public GameObject[] EnemyPrefabs;
    public List<EnemyBase> ActiveEnemies = new List<EnemyBase>();

    public void SpawnEnemy()
    {
        if (EnemyPrefabs.Length == 0 || GridManager == null) return;

        int RandomX = Random.Range(0, GridManager.Columns);
        int SpawnY = GridManager.Rows - 1;

        int RandomEnemyIndex = Random.Range(0, EnemyPrefabs.Length);
        GameObject PrefabToSpawn = EnemyPrefabs[RandomEnemyIndex];

        Vector2 SpawnPosition = GridManager.GetWorldPosition(RandomX, SpawnY);
        GameObject NewEnemyObj = Instantiate(PrefabToSpawn, SpawnPosition, Quaternion.identity);

        EnemyBase NewEnemy = NewEnemyObj.GetComponent<EnemyBase>();
        if (NewEnemy != null)
        {
            NewEnemy.Initialize(this, GridManager, RandomX, SpawnY);
            ActiveEnemies.Add(NewEnemy);
        }
    }

    public IEnumerator ProcessEnemiesTurnRoutine()
    {
        List<Coroutine> MoveCoroutines = new List<Coroutine>();

        for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
        {
            if (ActiveEnemies[i] == null)
            {
                ActiveEnemies.RemoveAt(i);
                continue;
            }
            MoveCoroutines.Add(StartCoroutine(ActiveEnemies[i].MoveRoutine()));
        }

        foreach(Coroutine MoveRoutine in MoveCoroutines)
        {
            yield return MoveRoutine;
        }

        SpawnEnemy();
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
