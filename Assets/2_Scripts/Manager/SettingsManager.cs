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

    public bool IsVibrationEnabled { get; private set; }

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
    }

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
}
