// UserCloudData.cs 전체를 아래 내용으로 교체하세요.

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserCloudData
{
    public int BestScore = 0;
    public int TotalPlayCount = 0;
    public int BestCombo = 0; // [추가]

    public Dictionary<string, bool> Achievements = new Dictionary<string, bool>();

    public static UserCloudData FromLocalPrefs(AchievementDatabase database)
    {
        var data = new UserCloudData();
        data.BestScore = PlayerPrefs.GetInt("BestScore", 0);
        data.TotalPlayCount = PlayerPrefs.GetInt("TotalPlayCount", 0);
        data.BestCombo = PlayerPrefs.GetInt("BestCombo", 0); // [추가]

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

    public static UserCloudData Merge(UserCloudData local, UserCloudData cloud)
    {
        var merged = new UserCloudData();

        merged.BestScore = Mathf.Max(local.BestScore, cloud.BestScore);
        merged.TotalPlayCount = Mathf.Max(local.TotalPlayCount, cloud.TotalPlayCount);
        merged.BestCombo = Mathf.Max(local.BestCombo, cloud.BestCombo); // [추가]

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

    public void ApplyToLocalPrefs()
    {
        PlayerPrefs.SetInt("BestScore", BestScore);
        PlayerPrefs.SetInt("TotalPlayCount", TotalPlayCount);
        PlayerPrefs.SetInt("BestCombo", BestCombo); // [추가]

        foreach (var kv in Achievements)
            PlayerPrefs.SetInt($"{kv.Key}_Unlocked", kv.Value ? 1 : 0);

        PlayerPrefs.Save();
    }
}
