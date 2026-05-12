using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GridManager GridManager;
    public EnemyManager EnemyManager;

    public GameObject[] ItemPrefabs;
    private List<ItemBase> ActiveItems = new List<ItemBase>();

    private PlayerController _player;
    private PlayerController CachedPlayer => _player != null ? _player : _player = FindFirstObjectByType<PlayerController>();

    [Header("Item Spawn Settings")]
    public int ItemSpawnScoreThreshold = 200;
    private int lastProcessedScore = 0;

    private void Start()
    {
        // 싱글톤 인스턴스에 접근하여 이벤트 구독
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
            GameObject NewItemObj = Instantiate(Prefab, SpawnPos, Quaternion.identity);
            ItemBase NewItem = NewItemObj.GetComponent<ItemBase>();

            if (NewItem != null)
            {
                NewItem.Initialize(this, SpawnX, SpawnY, SpawnPos);
                ActiveItems.Add(NewItem);
            }
        }
    }

    private void HandleScoreChanged(int currentTotalScore)
    {
        if (currentTotalScore == 0)
        {
            lastProcessedScore = 0;
            return;
        }

        int scoreGained = currentTotalScore - lastProcessedScore;

        if (scoreGained >= ItemSpawnScoreThreshold)
        {
            int spawnCount = scoreGained / ItemSpawnScoreThreshold;
            lastProcessedScore += spawnCount * ItemSpawnScoreThreshold;

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnRandomItem();
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
        foreach (ItemBase Item in ActiveItems)
        {
            if (Item.CurrentX == X && Item.CurrentY == Y) return Item;
        }
        return null;
    }

    public void RemoveItem(ItemBase item)
    {
        ActiveItems.Remove(item);
    }

    public void ClearItem()
    {
        foreach(ItemBase Item in ActiveItems)
        {
            if (Item != null)
            {
                Destroy(Item.gameObject);
            }
        }
        ActiveItems.Clear();
    }
}
