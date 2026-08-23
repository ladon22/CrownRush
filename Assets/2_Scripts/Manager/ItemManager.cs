using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GridManager GridManager;
    public EnemyManager EnemyManager;

    public GameObject[] ItemPrefabs;
    private List<ItemBase> ActiveItems = new List<ItemBase>();

    private PlayerController Player;
    private PlayerController CachedPlayer => Player != null ? Player : Player = FindFirstObjectByType<PlayerController>();

    [Header("Item Spawn Settings")]
    public int ItemSpawnScoreThreshold = 200;
    public int MaxActiveItems = 2;
    private int LastProcessedScore = 0;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    public void SpawnRandomItem()
    {
        if (ItemPrefabs.Length == 0) return;

        int SpawnX, SpawnY;
        if (GetValidSpawnPosition(out SpawnX, out SpawnY))
        {
            int RandomIndex = Random.Range(0, ItemPrefabs.Length);
            GameObject Prefab = ItemPrefabs[RandomIndex];
            Vector2 SpawnPos = GridManager.GetWorldPosition(SpawnX, SpawnY);
            GameObject NewItemObj = PoolManager.Instance.SpawnFromPool(Prefab, SpawnPos, Quaternion.identity);
            ItemBase NewItem = NewItemObj.GetComponent<ItemBase>();

            if (NewItem != null)
            {
                NewItem.Initialize(this, SpawnX, SpawnY, SpawnPos);
                GridManager.PlaceOccupant(NewItem, SpawnX, SpawnY);
                ActiveItems.Add(NewItem);

                if (TutorialHintManager.Instance != null)
                    TutorialHintManager.Instance.TryShowHint(TutorialHintID.Item);
            }
        }
    }

    private void HandleScoreChanged(int currentTotalScore)
    {
        if (currentTotalScore == 0)
        {
            LastProcessedScore = 0;
            return;
        }

        int scoreGained = currentTotalScore - LastProcessedScore;

        if (scoreGained >= ItemSpawnScoreThreshold)
        {
            int spawnCount = scoreGained / ItemSpawnScoreThreshold;
            LastProcessedScore += spawnCount * ItemSpawnScoreThreshold;

            for (int i = 0; i < spawnCount; i++)
            {
                if (ActiveItems.Count < MaxActiveItems)
                {
                    SpawnRandomItem();
                }
            }
        }
    }

    private bool GetValidSpawnPosition(out int SpawnX, out int SpawnY)
    {
        SpawnX = -1; SpawnY = -1;
        List<Vector2Int> EmptyTiles = new List<Vector2Int>();
        for (int Y = 0; Y < GridManager.Rows; Y++)
        {
            for (int X = 0; X < GridManager.Columns; X++)
            {
                if (CachedPlayer != null && CachedPlayer.CurrentX == X && CachedPlayer.CurrentY == Y)
                    continue;
                if (EnemyManager != null && EnemyManager.GetEnemyAtPosition(X, Y))
                    continue;
                if (GetItemAtPosition(X, Y) != null)
                    continue;
                EmptyTiles.Add(new Vector2Int(X, Y));
            }
        }

        if (EmptyTiles.Count > 0)
        {
            int RandomIndex = Random.Range(0, EmptyTiles.Count);
            SpawnX = EmptyTiles[RandomIndex].x;
            SpawnY = EmptyTiles[RandomIndex].y;
            return true;
        }
        return false;
    }

    public ItemBase GetItemAtPosition(int X, int Y)
    {
        return GridManager.GetOccupant(X, Y) as ItemBase;
    }

    public void RemoveItem(ItemBase item)
    {
        if (item != null)
        {
            GridManager.RemoveOccupant(item, item.CurrentX, item.CurrentY);
        }
        ActiveItems.Remove(item);
    }

    public void ClearItem()
    {
        foreach(ItemBase Item in ActiveItems)
        {
            if (Item != null)
            {
                GridManager.RemoveOccupant(Item, Item.CurrentX, Item.CurrentY);
                Item.KillTweens();
                PoolManager.Instance.ReturnToPool(Item.gameObject);
            }
        }
        ActiveItems.Clear();
    }
}
