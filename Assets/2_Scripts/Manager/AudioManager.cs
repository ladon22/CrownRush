using System.Collections.Generic;
using UnityEngine;

public enum BgmType
{
    Lobby,
    InGame
}

public enum SfxType
{
    Click,
    Close,
    PlayerMove,
    PlayerAttack,
    PlayerHit,
    EnemyDrop,
    EnemyDie,
    ItemConsume,
    GameOver
}

[System.Serializable]
public struct BgmTrack
{
    public BgmType Type;
    public AudioClip Clip;
}

[System.Serializable]
public struct SfxTrack
{
    public SfxType Type;
    public AudioClip Clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource BGMAudioSource;
    public AudioSource SFXAudioSource;

    [Header("Audio Clips")]
    public BgmTrack[] BgmTracks;
    public SfxTrack[] SfxTracks;

    private Dictionary<BgmType, AudioClip> BgmDictionary = new Dictionary<BgmType, AudioClip>();
    private Dictionary<SfxType, AudioClip> SfxDictionary = new Dictionary<SfxType, AudioClip>();

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
        InitializeDictionaries();
        LoadSettings();
    }

    private void InitializeDictionaries()
    {
        foreach (BgmTrack track in BgmTracks)
        {
            if (!BgmDictionary.ContainsKey(track.Type))
                BgmDictionary.Add(track.Type, track.Clip);
        }

        foreach (SfxTrack track in SfxTracks)
        {
            if (!SfxDictionary.ContainsKey(track.Type))
                SfxDictionary.Add(track.Type, track.Clip);
        }
    }

    private void LoadSettings()
    {
        IsBGMOn = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

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

    public void PlayBGM(BgmType type)
    {
        if (BGMAudioSource == null) return;
        if (BgmDictionary.TryGetValue(type, out AudioClip clip))
        {

            if (BGMAudioSource.clip == clip && BGMAudioSource.isPlaying) return;

            BGMAudioSource.clip = clip;
            BGMAudioSource.loop = true;
            BGMAudioSource.Play();
        }
    }

    public void StopBGM()
    {
        if (BGMAudioSource != null) BGMAudioSource.Stop();
    }

    public void PlaySFX(SfxType type, bool useRandomPitch = false)
    {
        if (SFXAudioSource == null) return;

        if (SfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            if (useRandomPitch)
            {
                SFXAudioSource.pitch = Random.Range(0.9f, 1.1f);
            }
            else
            {
                SFXAudioSource.pitch = 1.0f;
            }

            SFXAudioSource.PlayOneShot(clip);
        }
    }
}
