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

    [Header("SFX Pool")]
    [SerializeField] private int SfxPoolSize = 8;
    [SerializeField] private int SfxPoolMaxSize = 32;

    [Header("Audio Clips")]
    public BgmTrack[] BgmTracks;
    public SfxTrack[] SfxTracks;

    private Dictionary<BgmType, AudioClip> BgmDictionary = new Dictionary<BgmType, AudioClip>();
    private Dictionary<SfxType, AudioClip> SfxDictionary = new Dictionary<SfxType, AudioClip>();

    // 여러 SFX가 겹칠 때 pitch가 서로 간섭하지 않도록 소스를 나눠 씁니다.
    private List<AudioSource> SfxPool = new List<AudioSource>();
    private int NextSfxIndex = 0;

    private const string BgmKey = "Settings_BGM_On";
    private const string SfxKey = "Settings_SFX_On";

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
        InitializeSfxPool();
        LoadSettings();
    }

    private void InitializeSfxPool()
    {
        if (SFXAudioSource == null) return;

        SFXAudioSource.playOnAwake = false;
        SFXAudioSource.loop = false;
        SfxPool.Add(SFXAudioSource);

        for (int i = 1; i < SfxPoolSize; i++)
        {
            SfxPool.Add(CreateSfxSource(i));
        }
    }

    private AudioSource CreateSfxSource(int index)
    {
        GameObject obj = new GameObject($"SFX_Source_{index}");
        obj.transform.SetParent(SFXAudioSource.transform.parent, false);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = SFXAudioSource.outputAudioMixerGroup;
        source.spatialBlend = SFXAudioSource.spatialBlend;
        source.volume = SFXAudioSource.volume;
        source.mute = SFXAudioSource.mute;
        source.playOnAwake = false;
        source.loop = false;

        return source;
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
        IsBGMOn = PlayerPrefs.GetInt(BgmKey, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;

        if (BGMAudioSource != null) BGMAudioSource.mute = !IsBGMOn;
        SetSfxMute(!IsSFXOn);
    }

    public void ToggleBGM(bool isOn)
    {
        IsBGMOn = isOn;
        if (BGMAudioSource != null) BGMAudioSource.mute = !IsBGMOn;
        PlayerPrefs.SetInt(BgmKey, IsBGMOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX(bool isOn)
    {
        IsSFXOn = isOn;
        SetSfxMute(!IsSFXOn);
        PlayerPrefs.SetInt(SfxKey, IsSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetSfxMute(bool mute)
    {
        foreach (AudioSource source in SfxPool)
        {
            if (source != null) source.mute = mute;
        }
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

    public void PlaySFX(SfxType type, bool useRandomPitch = false)
    {
        if (SfxPool.Count == 0) return;

        if (SfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            AudioSource source = GetAvailableSfxSource();
            source.pitch = useRandomPitch ? Random.Range(0.9f, 1.1f) : 1.0f;
            source.clip = clip;
            source.Play();
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (AudioSource source in SfxPool)
        {
            if (!source.isPlaying) return source;
        }

        if (SfxPool.Count < SfxPoolMaxSize)
        {
            AudioSource newSource = CreateSfxSource(SfxPool.Count);
            newSource.mute = SfxPool[0].mute;
            SfxPool.Add(newSource);
            return newSource;
        }

        AudioSource fallback = SfxPool[NextSfxIndex];
        NextSfxIndex = (NextSfxIndex + 1) % SfxPool.Count;
        return fallback;
    }
}
