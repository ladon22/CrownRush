using UnityEngine;

[CreateAssetMenu(fileName = "NewSkinData", menuName = "GameData/Item/SkinData")]
public class SkinData : ItemData
{
    [Header("Skin Resource")]
    public Sprite SkinSprite;
    public Color SkinColor = Color.white;
    // 나중을 위해 애니메이터가 필요하다면 여기에 추가하면 됩니다.
    // public RuntimeAnimatorController SkinAnimator; 
}