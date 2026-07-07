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

    // SFXAudioSource는 AudioManager와 같은 GameObject에 있으므로,
    // Instantiate(SFXAudioSource, ...)로 복제하면 AudioManager 싱글턴까지 통째로 복제되어 버립니다.
    // 그래서 AudioSource만 담은 새 GameObject를 만들어 필요한 설정만 옮겨줍니다.
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
        IsBGMOn = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        if (BGMAudioSource != null) BGMAudioSource.mute = !IsBGMOn;
        SetSfxMute(!IsSFXOn);
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
        SetSfxMute(!IsSFXOn);
        PlayerPrefs.SetInt(SFX_KEY, IsSFXOn ? 1 : 0);
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

    public void StopBGM()
    {
        if (BGMAudioSource != null) BGMAudioSource.Stop();
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

    // 재생 중이지 않은 소스를 우선 반환합니다. 전부 사용 중이면 재생 중인 소리를 끊는 대신
    // (최대치까지는) 풀을 늘려서 새 소스를 만듭니다 — 소리를 가로채면 파형 중간이 끊겨 클릭음이 납니다.
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
