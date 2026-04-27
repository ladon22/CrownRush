using System;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
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
            if (data.Type == type)
            {
                if (PlayerPrefs.GetInt(data.AchievementID, 0) == 1) continue;

                if (currentValue >= data.TargetValue)
                {
                    UnlockAchievement(data);
                }
            }
        }
    }

    private void UnlockAchievement(AchievementEntry data)
    {
        PlayerPrefs.SetInt(data.AchievementID, 1);
        PlayerPrefs.Save();

        // 더 이상 여기서 UI를 직접 조작하지 않습니다. 
        // 대신 "업적 달성했어!" 라고 방송(Invoke)만 합니다.
        OnAchievementUnlocked?.Invoke(data);
    }
}