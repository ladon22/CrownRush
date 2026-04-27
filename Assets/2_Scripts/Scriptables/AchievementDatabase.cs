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

// 개별 업적 정보 (더 이상 ScriptableObject가 아닙니다!)
[System.Serializable]
public class AchievementEntry
{
    public string AchievementID;
    public AchievementType Type;
    public int TargetValue;

    [Header("UI Info")]
    public string Title;
    public string RewardText;

    [Header("Reward Info")]
    public RewardType RewardType;
    public string RewardID;
}

// 위의 '업적 항목'들을 묶어서 관리하는 단 하나의 '데이터베이스'
[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "GameData/AchievementDatabase")]
public class AchievementDatabase : ScriptableObject
{
    // 유니티 인스펙터에서 이 리스트 하나만 관리하면 끝납니다!
    public List<AchievementEntry> Achievements = new List<AchievementEntry>();
}