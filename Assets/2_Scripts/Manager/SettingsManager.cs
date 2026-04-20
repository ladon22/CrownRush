using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    private const string ScreenShakeKey = "Settings_ScreenShake";

    public bool IsScreenShakeEnabled { get; private set; }

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

        IsScreenShakeEnabled = PlayerPrefs.GetInt(ScreenShakeKey, 1) == 1;
    }

    public void SetScreenShake(bool value)
    {
        IsScreenShakeEnabled = value;
        PlayerPrefs.SetInt(ScreenShakeKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
