using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource BGMAudioSource;
    public AudioSource SFXAudioSource;

    private const string BGM_KEY = "Settings_BGM_On";
    private const string SFX_KEY = "Settings_SFX_On";

    public bool IsBGMOn { get; private set; }
    public bool IsSFXOn { get; private set; }

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

        LoadSettings();
    }

    private void LoadSettings()
    {
        IsBGMOn = PlayerPrefs.GetFloat(BGM_KEY, 1f) == 1;
        IsSFXOn = PlayerPrefs.GetFloat(SFX_KEY, 1f) == 1;

        if (BGMAudioSource != null) BGMAudioSource.mute = !IsBGMOn;
        if (SFXAudioSource != null) SFXAudioSource.mute = !IsSFXOn;
    }

    public void ToggleBGM(bool isOn)
    {
        IsBGMOn = isOn;
        if (BGMAudioSource != null) BGMAudioSource.mute = !IsBGMOn;
        PlayerPrefs.SetInt(BGM_KEY, IsBGMOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX(bool isOn)
    {
        IsSFXOn = isOn;
        if (SFXAudioSource != null) SFXAudioSource.mute = !IsSFXOn;
        PlayerPrefs.SetInt(SFX_KEY, IsSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 기존의 재생/정지 함수들은 그대로 유지합니다.
    public void PlayBGM(AudioClip clip)
    {
        if (BGMAudioSource == null || clip == null) return;
        BGMAudioSource.clip = clip;
        BGMAudioSource.loop = true;
        BGMAudioSource.Play();
    }

    public void StopBGM()
    {
        if (BGMAudioSource != null) BGMAudioSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXAudioSource == null || clip == null) return;
        SFXAudioSource.PlayOneShot(clip);
    }
}
