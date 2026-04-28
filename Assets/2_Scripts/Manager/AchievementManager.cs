using System;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    [Header("Achievement Database")]
    public AchievementDatabase Database;

    // [핵심] 업적이 달성되었음을 외부에 알리는 이벤트(방송국)
    public static event Action<AchievementEntry> OnAchievementUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // BootScene에서 생성된 후 파괴되지 않도록 설정
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
            // 행동 타입이 일치하는지 확인
            if (data.Type == type && data.RewardItem != null)
            {
                // 이미 해금된 스킨인지 확인 (RewardID를 기준으로 확인)
                string UnlockKey = $"{data.RewardItem.ItemID}_Unlocked";
                if (PlayerPrefs.GetInt(UnlockKey, 0) == 1) continue;

                // 목표치에 도달했다면 해금!
                if (currentValue >= data.TargetValue)
                {
                    UnlockAchievement(data, UnlockKey);
                }
            }
        }
    }

    private void UnlockAchievement(AchievementEntry data, string UnlockKey)
    {
        // 기기에 "이 스킨 해금됨(1)" 이라고 저장
        PlayerPrefs.SetInt(UnlockKey, 1);
        PlayerPrefs.Save();

        // 팝업 UI 등에게 해금 사실을 알림
        OnAchievementUnlocked?.Invoke(data);
    }
}