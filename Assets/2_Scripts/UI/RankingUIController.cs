using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RankingUIController : MonoBehaviour
{
    [Header("Panel & Animation")]
    [SerializeField] private GameObject RankingPanel;
    [SerializeField] private float PopDuration = 0.4f;
    [SerializeField] private Ease PopEase = Ease.OutBack;

    [Header("UI 연결 (전체 랭킹)")]
    public ScrollRect RankingScrollRect;
    public Transform ContentTransform;
    public GameObject RankEntryPrefab;

    [Header("UI 연결 (내 순위)")]
    public TextMeshProUGUI MyRankScoreText;
    [Header("Localization (다국어 포맷)")]
    public LocalizedString RankFormatString;
    public LocalizedString ScoreFormatString;
    public LocalizedString UnrankedString;

    private string CurrentRankFormat = "{0}위";
    private string CurrentScoreFormat = "{0}점";
    private string CurrentUnrankedText = "50위 밖";

    [Header("Recycling Settings")]
    public float ItemHeight = 101f;
    public int VisibleItemCount = 8;

    [Header("Entry Colors")]
    public Color OddRowColor = Color.white;
    public Color EvenRowColor = Color.white;

    [Header("Loading Animation")]
    [SerializeField] private TextMeshProUGUI LoadingText;
    [SerializeField] private LocalizedString LoadingBaseString;

    [Header("Offline Warning")]
    [SerializeField] private ToastUI OfflineWarningPanel;

    private RectTransform ContentRect;
    private List<RankEntryView> PoolItems = new List<RankEntryView>();
    private List<RectTransform> PoolItemRects = new List<RectTransform>();

    private List<RankData> RankDataList = new List<RankData>();
    private Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
    private HashSet<string> DownloadingUrls = new HashSet<string>();
    private int LastTopIndex = -1;

    private Tween LoadingDotTween;
    private string LocalizedLoadingText = "Loading";

    private void Awake()
    {
        if (OfflineWarningPanel != null)
            OfflineWarningPanel.gameObject.SetActive(false);

        ContentRect = ContentTransform.GetComponent<RectTransform>();

        if (RankingScrollRect != null)
        {
            RankingScrollRect.onValueChanged.AddListener(OnScroll);
        }

        for (int i = 0; i < VisibleItemCount; i++)
        {
            GameObject obj = Instantiate(RankEntryPrefab, ContentTransform);

            RankEntryView entry = obj.GetComponent<RankEntryView>();
            PoolItems.Add(entry);

            RectTransform rect = obj.GetComponent<RectTransform>();
            PoolItemRects.Add(rect);

            obj.SetActive(false);
        }
    }

    public void ShowRanking()
    {
        if (RankingPanel != null && RankingPanel.activeInHierarchy) return;

        gameObject.SetActive(true);
        if (!FireBaseManager.Instance.UseFirebase || !IsNetworkAvailable())
        {
            ShowOfflineWarning();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();
            
        if (RankingPanel != null)
        {
            RankingPanel.gameObject.SetActive(true);
            RankingPanel.transform.localScale = Vector3.zero;
            RankingPanel.transform.DOKill();
            RankingPanel.transform.DOScale(Vector3.one, PopDuration).SetEase(PopEase).SetUpdate(true);
        }

        if (!RankFormatString.IsEmpty) CurrentRankFormat = RankFormatString.GetLocalizedString();
        if (!ScoreFormatString.IsEmpty) CurrentScoreFormat = ScoreFormatString.GetLocalizedString();
        if (!UnrankedString.IsEmpty) CurrentUnrankedText = UnrankedString.GetLocalizedString();

        RefreshRanking();

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(HideRanking);
    }

    public void HideRanking()
    {
        if (RankingPanel != null)
        {
            RankingPanel.transform.DOScale(Vector3.zero, PopDuration * 0.7f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    if (GameManager.Instance != null)
                        GameManager.Instance.InputController.PopInputLock();
                });
        }
        else
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PopInputLock();
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    public void RefreshRanking()
    {
        RankDataList.Clear();
        LastTopIndex = -1;
        ContentRect.anchoredPosition = Vector2.zero;
        foreach (var item in PoolItems) item.gameObject.SetActive(false);

        LoadRankingData();
    }

    private bool IsNetworkAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void ShowOfflineWarning()
    {
        if (RankingPanel != null)
            RankingPanel.gameObject.SetActive(false);

        if (OfflineWarningPanel == null) return;
        OfflineWarningPanel.Show(() =>
        { 
            gameObject.SetActive(false);
        });
    }

    private async void LoadRankingData()
    {
        StartLoadingAnimation();

        if (FireBaseManager.Instance == null || !FireBaseManager.Instance.UseFirebase)
        {
            LoadDummyDataForEditor();
            return;
        }

        if (!IsNetworkAvailable())
        {
            if (LoadingText != null)
            {
                LoadingDotTween?.Kill();
                LoadingText.text = "인터넷 연결을 확인해주세요.";
            }
            return;
        }

        RankDataList = await FireBaseManager.Instance.GetTopRankingsAsync(50);

        string myUid = FireBaseManager.Instance.GetCurrentUserUid();
        bool amIinTop50 = RankDataList.Exists(r => r.Uid == myUid);

        if (amIinTop50)
        {
            RankData myData = RankDataList.Find(r => r.Uid == myUid);
            UpdateMyRankUI(myData.Rank, myData.Name, myData.Score, myData.PhotoUrl);
        }
        else
        {
            RankData myRank = await FireBaseManager.Instance.GetMyRankAsync();
            if (myRank != null)
                UpdateMyRankUI(myRank.Rank, myRank.Name, myRank.Score, myRank.PhotoUrl);
            else
                UpdateMyRankUI(-1, "", "0", "");
        }

        foreach (RankData data in RankDataList)
        {
            if (!string.IsNullOrEmpty(data.PhotoUrl)
                && !ImageCache.ContainsKey(data.PhotoUrl)
                && !DownloadingUrls.Contains(data.PhotoUrl))
            {
                StartCoroutine(LoadProfileImageRoutine(data.PhotoUrl));
            }
        }

        ContentRect.sizeDelta = new Vector2(
            ContentRect.sizeDelta.x,
            RankDataList.Count * ItemHeight);
        ContentRect.anchoredPosition = Vector2.zero;
        LastTopIndex = -1;
        OnScroll(Vector2.zero);
        StopLoadingAnimation();
    }
    private void UpdateMyRankUI(int rank, string name, string score, string photoUrl)
    {
        if (MyRankScoreText == null) return;

        int parsedScore = 0;
        int.TryParse(score, out parsedScore);
        string formattedScore = parsedScore.ToString("N0");

        if (MyRankScoreText != null)
        {
            string rankStr = rank > 0 ? string.Format(CurrentRankFormat, rank) : CurrentUnrankedText;
            string scoreStr = string.Format(CurrentScoreFormat, formattedScore);

            MyRankScoreText.text = $"{rankStr} <color=#AAAAAA>/</color> {scoreStr}";
        }
    }

    private void OnScroll(Vector2 scrollPos)
    {
        if (RankDataList.Count == 0) return;

        float scrollY = ContentRect.anchoredPosition.y;
        int topIndex = Mathf.FloorToInt(scrollY / ItemHeight);
        topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, RankDataList.Count - VisibleItemCount));

        if (topIndex != LastTopIndex)
        {
            UpdateVisibleItems(topIndex);
            LastTopIndex = topIndex;
        }
    }

    private void UpdateVisibleItems(int topIndex)
    {
        for (int i = 0; i < VisibleItemCount; i++)
        {
            int dataIndex = topIndex + i;

            if (dataIndex < RankDataList.Count)
            {
                RankEntryView entry = PoolItems[i];
                entry.gameObject.SetActive(true);

                PoolItemRects[i].anchoredPosition = new Vector2(0, -dataIndex * ItemHeight);

                RankData data = RankDataList[dataIndex];
                Sprite cachedSprite = ImageCache.TryGetValue(data.PhotoUrl, out Sprite sp) ? sp : null;
                entry.Bind(data, CurrentScoreFormat, cachedSprite);
                entry.SetBackgroundColor(dataIndex % 2 == 0 ? OddRowColor : EvenRowColor);
            }
            else
            {
                PoolItems[i].gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator LoadProfileImageRoutine(string url)
    {
        DownloadingUrls.Add(url);
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            if (!ImageCache.ContainsKey(url)) ImageCache.Add(url, newSprite);

            LastTopIndex = -1;
            OnScroll(Vector2.zero);
        }
        DownloadingUrls.Remove(url);
    }

    private void LoadDummyDataForEditor()
    {
        RankDataList.Clear();
        for (int i = 0; i < 30; i++)
        {
            RankDataList.Add(new RankData
            {
                Rank = i + 1,
                Name = "테스터 " + (i + 1),
                Score = (9999 - (i * 150)).ToString(),
                PhotoUrl = ""
            });
        }

        DG.Tweening.DOVirtual.DelayedCall(2.0f, () =>
        {
            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, RankDataList.Count * ItemHeight);
            ContentRect.anchoredPosition = Vector2.zero;
            LastTopIndex = -1;
            OnScroll(Vector2.zero);

            StopLoadingAnimation();
        });
    }


    private void StartLoadingAnimation()
    {
        if (LoadingText == null) return;

        LoadingText.gameObject.SetActive(true);
        LoadingDotTween?.Kill();
        LocalizedLoadingText = LoadingBaseString.GetLocalizedString();

        int dotCount = 0;
        LoadingDotTween = DOTween.To(() => dotCount, x => dotCount = x, 3, 1.5f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                string dots = new string('.', dotCount);
                LoadingText.text = $"{LocalizedLoadingText}{dots}";
            });
    }

    private void StopLoadingAnimation()
    {
        LoadingDotTween?.Kill();
        if (LoadingText != null) LoadingText.gameObject.SetActive(false);
    }
}