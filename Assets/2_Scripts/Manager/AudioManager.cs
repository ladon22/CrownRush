using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource BGMAudioSource;
    public AudioSource SFXAudioSource;

    private const string BGMVolumeKey = "Settings_BGMVolume";
    private const string SFXVolumeKey = "Settings_SFXVolume";

    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

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

        LoadVolumes();
    }

    private void LoadVolumes()
    {
        BGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        ApplyBGMVolume();
        ApplySFXVolume();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
        ApplyBGMVolume();
        PlayerPrefs.SetFloat(BGMVolumeKey, BGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        ApplySFXVolume();
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.Save();
    }

    private void ApplyBGMVolume()
    {
        if (BGMAudioSource != null)
            BGMAudioSource.volume = BGMVolume;
    }

    private void ApplySFXVolume()
    {
        if (SFXAudioSource != null)
            SFXAudioSource.volume = SFXVolume;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (BGMAudioSource == null || clip == null) return;
        BGMAudioSource.clip = clip;
        BGMAudioSource.loop = true;
        BGMAudioSource.Play();
    }

    public void StopBGM()
    {
        if (BGMAudioSource != null)
            BGMAudioSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXAudioSource == null || clip == null) return;
        SFXAudioSource.PlayOneShot(clip);
    }
}
