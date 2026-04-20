using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language
{
    Korean = 0,
    English = 1
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    private const string LanguageKey = "Settings_Language";

    public Language CurrentLanguage { get; private set; }
    public Action OnLanguageChanged;

    // 인덱스 0 = 한국어, 1 = 영어
    private readonly Dictionary<string, string[]> LocalizationTable = new Dictionary<string, string[]>()
    {
        { "btn_start",              new[] { "시작",       "Start"        } },
        { "btn_retry",              new[] { "재시작",      "Retry"        } },
        { "btn_home",               new[] { "홈",         "Home"         } },
        { "settings_title",         new[] { "설정",       "Settings"     } },
        { "settings_bgm",           new[] { "배경음악",    "BGM"          } },
        { "settings_sfx",           new[] { "효과음",      "SFX"          } },
        { "settings_language",      new[] { "언어",       "Language"     } },
        { "settings_screenshake",   new[] { "화면 흔들림",  "Screen Shake" } },
    };

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
            return;
        }

        CurrentLanguage = (Language)PlayerPrefs.GetInt(LanguageKey, (int)Language.Korean);
    }

    public void SetLanguage(Language lang)
    {
        if (CurrentLanguage == lang) return;

        CurrentLanguage = lang;
        PlayerPrefs.SetInt(LanguageKey, (int)CurrentLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (LocalizationTable.TryGetValue(key, out string[] values))
        {
            int index = (int)CurrentLanguage;
            if (index >= 0 && index < values.Length)
                return values[index];
        }

        Debug.LogWarning($"[LocalizationManager] 키를 찾을 수 없습니다: {key}");
        return key;
    }
}
