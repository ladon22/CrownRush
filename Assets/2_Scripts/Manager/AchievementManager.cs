using System;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    [Header("Achievement Database")]
    public AchievementDatabase Database;

    public static event Action<AchievementEntry> OnAchievementUnlocked;

    public static event Action OnSkinEquipped;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReportProgress(AchievementType type, int currentValue)
    {
        if (Database == null) return;

        foreach (AchievementEntry data in Database.Achievements)
        {
            if (data.Type == type && data.RewardItem != null)
            {
                if (data.RewardItem.ItemID == "skin_default") continue;
                string UnlockKey = $"{data.RewardItem.ItemID}_Unlocked";
                if (PlayerPrefs.GetInt(UnlockKey, 0) == 1) continue;

                if (currentValue >= data.TargetValue)
                {
                    UnlockAchievement(data, UnlockKey);
                }
            }
        }
    }

    private void UnlockAchievement(AchievementEntry data, string UnlockKey)
    {
        PlayerPrefs.SetInt(UnlockKey, 1);
        PlayerPrefs.Save();
        
        OnAchievementUnlocked?.Invoke(data);
       
        if (FireBaseManager.Instance != null && FireBaseManager.Instance.IsLoggedIn)
            FireBaseManager.Instance.SaveAchievement(data.RewardItem.ItemID);
    }

    public void EquipSkin(string skinID)
    {
        PlayerPrefs.SetString("Equipped_Skin", skinID);
        PlayerPrefs.Save();

        OnSkinEquipped?.Invoke();
    }
}