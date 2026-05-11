using System.Collections.Generic;
using UnityEngine;
public enum EnemyPieceType { Linear, Dash, Diagonal, Knight, Queen }

// 각 적 기물의 스킨 정보를 담는 구조체
[System.Serializable]
public struct EnemySkinData
{
    public EnemyPieceType Type;
    public Sprite EnemySprite;
}
[CreateAssetMenu(fileName = "NewSkinData", menuName = "GameData/Item/SkinData")]
public class SkinData : ItemData
{
    [Header("Board Skin")]
    public Sprite LightTileSprite; // 보드판 밝은 칸
    public Sprite DarkTileSprite;  // 보드판 어두운 칸
    public Color FrameColor = Color.white;

    [Header("Player Skin")]
    public Sprite PlayerSprite;
    public Color PlayerColor = Color.white;

    [Header("Enemy Skins")]
    public Color EnemyColor = Color.white;
    public List<EnemySkinData> EnemySkins = new List<EnemySkinData>();

    // 입력받은 적 타입에 맞는 스킨 데이터를 뽑아주는 헬퍼 함수
    public EnemySkinData GetEnemySkin(EnemyPieceType type)
    {
        foreach (var skin in EnemySkins)
        {
            if (skin.Type == type) return skin;
        }
        return new EnemySkinData(); // 못 찾았을 경우 빈 데이터 반환
    }
}