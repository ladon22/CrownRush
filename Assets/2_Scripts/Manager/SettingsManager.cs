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

    // 안드로이드 콜드 스타트 직후에는 그래픽/프레임 페이싱 파이프라인이 아직
    // 준비되지 않아 Awake()에서의 targetFrameRate 적용이 반영되지 않는 경우가 있어,
    // 부팅 후 한 번 더 재적용해 최초 실행 세션에서도 실제 프레임레이트가 어긋나지 않도록 한다.
    private IEnumerator ReapplyFrameRateAfterStartup()
    {
        yield return new WaitForSeconds(1.0f);
        ApplyFrameRate(CurrentFrameRate);
    }
    //###################### ���� ######################
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
    //###################### ������ ######################

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
