using System.Collections;
using UnityEngine;
using Solo.MOST_IN_ONE;

public enum VibrateType
{
    Light,
    Medium, 
    Heavy
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    private const string VibrationKey = "Settings_Vibration";
    private const string FrameRateKey = "Settings_FrameRate";


    public bool IsVibrationEnabled { get; private set; }
    public int CurrentFrameRate { get; private set; }

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

        IsVibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        MOST_HapticFeedback.HapticsEnabled = IsVibrationEnabled;

        CurrentFrameRate = PlayerPrefs.GetInt(FrameRateKey, 60);
        ApplyFrameRate(CurrentFrameRate);
        StartCoroutine(ReapplyFrameRateAfterStartup());
    }
    private IEnumerator ReapplyFrameRateAfterStartup()
    {
        yield return new WaitForSeconds(1.0f);
        ApplyFrameRate(CurrentFrameRate);
    }

    //###################### 진동 ######################
    public void SetVibration(bool value)
    {
        IsVibrationEnabled = value;
        PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
        PlayerPrefs.Save();

        MOST_HapticFeedback.HapticsEnabled = IsVibrationEnabled;
    }

    public void Vibrate(VibrateType type)
    {
        if (IsVibrationEnabled)
        {
#if UNITY_ANDROID || UNITY_IOS
            switch (type)
            {
                case VibrateType.Light:
                    MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.LightImpact);
                    break;

                case VibrateType.Medium:
                    MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
                    break;

                case VibrateType.Heavy:
                    MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
                    break;
            }
#endif
        }
    }
    //###################### 프레임레이트 ######################

    public void SetFrameRate(int fps)
    {
        CurrentFrameRate = fps;
        PlayerPrefs.SetInt(FrameRateKey, fps);
        PlayerPrefs.Save();

        ApplyFrameRate(CurrentFrameRate);
    }


    private void ApplyFrameRate(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

}
