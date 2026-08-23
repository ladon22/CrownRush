using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public GameObject Prefab;
    public int InitialSize = 5;
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Pre-warm Settings")]
    public List<PoolItem> ItemsToPrewarm;

    private Dictionary<string, Queue<GameObject>> PoolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(InitializePoolsRoutine());
    }

    public IEnumerator InitializePoolsRoutine()
    {
        int countPerFrame = 0;

        foreach (PoolItem Item in ItemsToPrewarm)
        {
            if (Item.Prefab == null) continue;

            string Key = Item.Prefab.name;

            if (PoolDictionary.ContainsKey(Key)) continue;

            PoolDictionary.Add(Key, new Queue<GameObject>());

            GameObject PoolParent = new GameObject($"{Key}_Pool");
            PoolParent.transform.SetParent(this.transform);

            for (int i = 0; i < Item.InitialSize; i++)
            {
                GameObject NewObj = Instantiate(Item.Prefab, PoolParent.transform);
                NewObj.name = Item.Prefab.name;
                NewObj.SetActive(false);

                PoolDictionary[Key].Enqueue(NewObj);

                countPerFrame++;

                if (countPerFrame >= 3)
                {
                    countPerFrame = 0;
                    yield return null;
                }
            }
        }
    }

    public GameObject SpawnFromPool(GameObject Prefab, Vector3 Position, Quaternion Rotation)
    {
        string Key = Prefab.name;

        if (!PoolDictionary.ContainsKey(Key))
        {
            PoolDictionary.Add(Key, new Queue<GameObject>());
        }

        GameObject ObjectToSpawn;

        if (PoolDictionary[Key].Count > 0)
        {
            ObjectToSpawn = PoolDictionary[Key].Dequeue();
            ObjectToSpawn.transform.position = Position;
            ObjectToSpawn.transform.rotation = Rotation;
            ObjectToSpawn.SetActive(true);
        }
        else
        {
            ObjectToSpawn = Instantiate(Prefab, Position, Rotation);
            ObjectToSpawn.name = Prefab.name;
        }

        return ObjectToSpawn;
    }

    public void ReturnToPool(GameObject ObjectToReturn)
    {
        string Key = ObjectToReturn.name;

        if (!PoolDictionary.ContainsKey(Key))
        {
            PoolDictionary.Add(Key, new Queue<GameObject>());
        }

        ObjectToReturn.SetActive(false);
        PoolDictionary[Key].Enqueue(ObjectToReturn);
    }
}
