using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    None,
    PlayerSkin,
    BoardTheme,
    Coin
}

public enum AchievementType
{
    Score,
    Combo,
    KillQueen,
    PlayCount
}

[System.Serializable]
public class AchievementEntry
{
    public string AchievementID;
    public AchievementType Type;
    public int TargetValue;

    [Header("UI Info")]
    public string TitleKey;

    [Header("Reward Item Data")]
    public ItemData RewardItem; 
}

[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "GameData/AchievementDatabase")]
public class AchievementDatabase : ScriptableObject
{
    public List<AchievementEntry> Achievements = new List<AchievementEntry>();
}