using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Firebase ↔ 로컬 간 데이터 운반용 클래스.
/// 로그인 직후 복원 시에만 사용됩니다.
///
/// 게임오버마다 생성하지 않습니다.
/// 각 저장은 FireBaseManager의 개별 메서드로 처리합니다.
///   SaveScoreToDatabase() — 신기록 달성 시
///   SavePlayCount()       — 매 게임오버 시
///   SaveAchievement()     — 업적 달성 시
/// </summary>
[System.Serializable]
public class UserCloudData
{
    public int BestScore = 0;
    public int TotalPlayCount = 0;

    // key: skinID (예: "skin_001"), value: 잠금 해제 여부
    public Dictionary<string, bool> Achievements = new Dictionary<string, bool>();

    // ══════════════════════════════════════════════════════
    // 로컬 → UserCloudData 생성 (복원 시 로컬 데이터 수집용)
    // ══════════════════════════════════════════════════════

    public static UserCloudData FromLocalPrefs(AchievementDatabase database)
    {
        var data = new UserCloudData();

        data.BestScore = PlayerPrefs.GetInt("BestScore", 0);
        data.TotalPlayCount = PlayerPrefs.GetInt("TotalPlayCount", 0);

        if (database != null)
        {
            foreach (AchievementEntry entry in database.Achievements)
            {
                if (entry.RewardItem == null) continue;
                if (entry.RewardItem.ItemID == "skin_default") continue;

                string key = entry.RewardItem.ItemID;
                bool isUnlocked = PlayerPrefs.GetInt($"{key}_Unlocked", 0) == 1;
                data.Achievements[key] = isUnlocked;
            }
        }

        return data;
    }

    // ══════════════════════════════════════════════════════
    // 병합 (항상 유저에게 유리한 방향으로)
    // ══════════════════════════════════════════════════════

    public static UserCloudData Merge(UserCloudData local, UserCloudData cloud)
    {
        var merged = new UserCloudData();

        // 점수/횟수는 더 높은 값 사용
        merged.BestScore = Mathf.Max(local.BestScore, cloud.BestScore);
        merged.TotalPlayCount = Mathf.Max(local.TotalPlayCount, cloud.TotalPlayCount);

        // 업적은 둘 중 하나라도 해제됐으면 해제 유지
        merged.Achievements = new Dictionary<string, bool>();

        foreach (var kv in local.Achievements)
            merged.Achievements[kv.Key] = kv.Value;

        foreach (var kv in cloud.Achievements)
        {
            if (!merged.Achievements.ContainsKey(kv.Key))
                merged.Achievements[kv.Key] = kv.Value;
            else
                merged.Achievements[kv.Key] = merged.Achievements[kv.Key] || kv.Value;
        }

        return merged;
    }

    // ══════════════════════════════════════════════════════
    // 로컬 반영 (복원 시 1회 호출)
    // ══════════════════════════════════════════════════════

    public void ApplyToLocalPrefs()
    {
        PlayerPrefs.SetInt("BestScore", BestScore);
        PlayerPrefs.SetInt("TotalPlayCount", TotalPlayCount);

        foreach (var kv in Achievements)
            PlayerPrefs.SetInt($"{kv.Key}_Unlocked", kv.Value ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("[UserCloudData] 로컬 데이터 복원 완료.");
    }
}
