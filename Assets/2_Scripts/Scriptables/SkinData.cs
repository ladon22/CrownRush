using System.Collections.Generic;
using UnityEngine;
public enum EnemyPieceType { Pawn, Rook, Bishop, Knight, Queen }

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
    public Sprite LightTileSprite;
    public Sprite DarkTileSprite;
    public Sprite FrameSprite;
    public Color FrameColor = Color.white;

    [Header("Player Skin")]
    public Sprite PlayerSprite;
    public Color PlayerColor = Color.white;

    [Header("Enemy Skins")]
    public Color EnemyColor = Color.white;
    public List<EnemySkinData> EnemySkins = new List<EnemySkinData>();


    public EnemySkinData GetEnemySkin(EnemyPieceType type)
    {
        foreach (var skin in EnemySkins)
        {
            if (skin.Type == type) return skin;
        }
        return new EnemySkinData();
    }
}