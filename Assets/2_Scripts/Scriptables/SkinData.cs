using System.Collections.Generic;
using UnityEngine;
public enum EnemyPieceType { Pawn, Rook, Bishop, Knight, Queen }

// 적 기물 하나의 스킨 데이터를 담는 구조체
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
    public Sprite LightTileSprite; // 체스판의 밝은 칸
    public Sprite DarkTileSprite;  // 체스판의 어두운 칸
    public Sprite FrameSprite;
    public Color FrameColor = Color.white;

    [Header("Player Skin")]
    public Sprite PlayerSprite;
    public Color PlayerColor = Color.white;

    [Header("Enemy Skins")]
    public Color EnemyColor = Color.white;
    public List<EnemySkinData> EnemySkins = new List<EnemySkinData>();

    // 입력받은 적 타입에 맞는 스킨 데이터를 찾아주는 헬퍼 함수
    public EnemySkinData GetEnemySkin(EnemyPieceType type)
    {
        foreach (var skin in EnemySkins)
        {
            if (skin.Type == type) return skin;
        }
        return new EnemySkinData(); // 못 찾았을 경우 빈 데이터 반환
    }
}