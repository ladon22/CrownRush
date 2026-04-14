using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public GameObject Prefab;
    public int InitialSize = 5; // 기본으로 5개씩 만들어둡니다.
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Pre-warm Settings")]
    public List<PoolItem> ItemsToPrewarm; // 인스펙터에 노출될 리스트

    private Dictionary<string, Queue<GameObject>> PoolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (PoolItem Item in ItemsToPrewarm)
        {
            if (Item.Prefab == null) continue;

            string Key = Item.Prefab.name;
            PoolDictionary.Add(Key, new Queue<GameObject>());

            // 🌟 하이어라키 창이 지저분해지지 않도록 묶어줄 빈 부모 오브젝트 생성 (예: "LinearEnemy_Pool")
            GameObject PoolParent = new GameObject($"{Key}_Pool");
            PoolParent.transform.SetParent(this.transform);

            // 지정한 개수만큼 미리 만들어서 큐에 넣습니다.
            for (int i = 0; i < Item.InitialSize; i++)
            {
                // 부모 밑에 생성
                GameObject NewObj = Instantiate(Item.Prefab, PoolParent.transform);
                NewObj.name = Item.Prefab.name; // "(Clone)" 이름 떼기
                NewObj.SetActive(false); // 꺼진 상태로 대기열에 추가

                PoolDictionary[Key].Enqueue(NewObj);
            }
        }
    }

    public GameObject SpawnFromPool(GameObject Prefab, Vector3 Position, Quaternion Rotation)
    {
        string Key = Prefab.name;

        // 딕셔너리에 이 프리팹의 큐가 없다면 하나 만들어줍니다.
        if (!PoolDictionary.ContainsKey(Key))
        {
            PoolDictionary.Add(Key, new Queue<GameObject>());
        }

        GameObject ObjectToSpawn;

        // 큐에 남은 재고가 있다면 꺼내 쓰고, 없다면 새로 하나 만듭니다(Instantiate).
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
            // 풀로 돌아갈 때를 대비해 (Clone) 글자를 떼고 원본 이름을 맞춰줍니다.
            ObjectToSpawn.name = Prefab.name;
        }

        return ObjectToSpawn;
    }

    // 2. 풀로 반납하기 (Destroy 대체)
    public void ReturnToPool(GameObject ObjectToReturn)
    {
        string Key = ObjectToReturn.name;

        // 혹시 모를 에러 방지 (풀에 등록되지 않은 객체라면)
        if (!PoolDictionary.ContainsKey(Key))
        {
            PoolDictionary.Add(Key, new Queue<GameObject>());
        }

        // 오브젝트를 끄고 대기열 큐에 다시 집어넣습니다.
        ObjectToReturn.SetActive(false);
        PoolDictionary[Key].Enqueue(ObjectToReturn);
    }
}
