using UnityEngine;

[CreateAssetMenu(fileName = "NewBoardData", menuName = "GameData/Item/BoardThemeData")]
public class BoardThemeData : ItemData
{
    [Header("Board Prefabs")]
    // 회원님의 BoardRenderer 코드를 보면 프리팹을 쓰고 계시므로, 프리팹을 받습니다!
    public GameObject LightTilePrefab;
    public GameObject DarkTilePrefab;
}
