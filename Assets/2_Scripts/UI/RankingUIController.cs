using DG.Tweening;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RankData
{
    public int Rank;
    public string Uid;
    public string Name;
    public string Score;
    public string PhotoUrl;
}

public class RankingUIController : MonoBehaviour
{
    [Header("Panel & Animation")]
    [SerializeField] private RectTransform PanelRect; // 랭킹 창 전체를 감싸는 RectTransform
    [SerializeField] private float PopDuration = 0.4f;
    [SerializeField] private Ease PopEase = Ease.OutBack;

    [Header("UI 연결 (전체 랭킹)")]
    public ScrollRect RankingScrollRect;
    public Transform ContentTransform;
    public GameObject RankEntryPrefab;

    [Header("UI 연결 (내 순위)")]
    public TextMeshProUGUI MyRankScoreText; // "24위 · 342점"을 띄울 텍스트

    [Header("Localization (다국어 포맷)")]
    public LocalizedString RankFormatString;  // 예: "{0}위" 또는 "Rank {0}"
    public LocalizedString ScoreFormatString; // 예: "{0}점" 또는 "{0} Pts"
    public LocalizedString UnrankedString;    // 예: "50위 밖" 또는 "Unranked"

    // 캐싱해둘 다국어 문자열 변수
    private string currentRankFormat = "{0}위";
    private string currentScoreFormat = "{0}점";
    private string currentUnrankedText = "50위 밖";

    [Header("Recycling Settings")]
    public float ItemHeight = 101f;
    public int VisibleItemCount = 8;

    [Header("Entry Colors")]
    public Color OddRowColor = Color.white;
    public Color EvenRowColor = Color.white;

    [Header("Loading Animation")]
    [SerializeField] private TextMeshProUGUI LoadingText; // "로딩 중" 글자가 써질 텍스트
    [SerializeField] private LocalizedString LoadingBaseString; // "로딩 중"이라는 기본 다국어 키

    private RectTransform ContentRect;
    private List<RankEntryView> PoolItems = new List<RankEntryView>();

    private List<RankData> RankDataList = new List<RankData>();
    private Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
    private HashSet<string> DownloadingUrls = new HashSet<string>();
    private int LastTopIndex = -1;

    private Tween loadingDotTween;
    private string localizedLoadingText = "Loading"; // 캐싱용

    private void Awake()
    {
        ContentRect = ContentTransform.GetComponent<RectTransform>();

        if (RankingScrollRect != null)
        {
            RankingScrollRect.onValueChanged.AddListener(OnScroll);
        }

        for (int i = 0; i < VisibleItemCount; i++)
        {
            GameObject obj = Instantiate(RankEntryPrefab, ContentTransform);
            RankEntryView entry = obj.GetComponent<RankEntryView>();
            obj.SetActive(false);
            PoolItems.Add(entry);
        }
    }

    public void ShowRanking()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushUIBlock();

        gameObject.SetActive(true);

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase).SetUpdate(true);
        }

        if (!RankFormatString.IsEmpty) currentRankFormat = RankFormatString.GetLocalizedString();
        if (!ScoreFormatString.IsEmpty) currentScoreFormat = ScoreFormatString.GetLocalizedString();
        if (!UnrankedString.IsEmpty) currentUnrankedText = UnrankedString.GetLocalizedString();

        RefreshRanking();
    }

    public void HideRanking()
    {
        if (PanelRect != null)
        {
            PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    if (GameManager.Instance != null)
                        GameManager.Instance.InputController.PopUIBlock();
                });
        }
        else
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PopUIBlock();
        }
    }

    // 새로고침 버튼과 ShowRanking에서 공통으로 쓰는 함수
    public void RefreshRanking()
    {
        RankDataList.Clear();
        LastTopIndex = -1;
        ContentRect.anchoredPosition = Vector2.zero;
        foreach (var item in PoolItems) item.gameObject.SetActive(false);

        LoadRankingData();
    }

    private async void LoadRankingData()
    {
        StartLoadingAnimation();

        if (FireBaseManager.Instance != null && FireBaseManager.Instance.bUseFirebase == false)
        {
            LoadDummyDataForEditor();
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
            // 50위 밖 — 별도 조회
            RankData myRank = await FireBaseManager.Instance.GetMyRankAsync();
            if (myRank != null)
                UpdateMyRankUI(myRank.Rank, myRank.Name, myRank.Score, myRank.PhotoUrl);
            else
                UpdateMyRankUI(-1, "", "0", "");
        }

        // 3. 프로필 이미지 캐싱 시작
        foreach (RankData data in RankDataList)
        {
            if (!string.IsNullOrEmpty(data.PhotoUrl)
                && !ImageCache.ContainsKey(data.PhotoUrl)
                && !DownloadingUrls.Contains(data.PhotoUrl))
            {
                StartCoroutine(LoadProfileImageRoutine(data.PhotoUrl));
            }
        }

        // 4. 스크롤 뷰 갱신
        ContentRect.sizeDelta = new Vector2(
            ContentRect.sizeDelta.x,
            RankDataList.Count * ItemHeight);
        ContentRect.anchoredPosition = Vector2.zero;
        LastTopIndex = -1;
        OnScroll(Vector2.zero);
        StopLoadingAnimation();
    }

    // 내 상단 UI 갱신 (점·콤마·다국어 적용)
    private void UpdateMyRankUI(int rank, string name, string score, string photoUrl)
    {
        if (MyRankScoreText == null) return;

        int parsedScore = 0;
        int.TryParse(score, out parsedScore);
        string formattedScore = parsedScore.ToString("N0"); // 천 단위 콤마 추가

        if (MyRankScoreText != null)
        {
            string rankStr = rank > 0 ? string.Format(currentRankFormat, rank) : currentUnrankedText;
            string scoreStr = string.Format(currentScoreFormat, formattedScore);

            // "24위 · 342점" 형태로 결합
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

                // 위치 배치
                RectTransform rt = entry.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -dataIndex * ItemHeight);

                // [수정] transform.Find() 제거 → RankEntryView.Bind() 호출
                RankData data = RankDataList[dataIndex];
                Sprite cachedSprite = ImageCache.TryGetValue(data.PhotoUrl, out Sprite sp) ? sp : null;
                entry.Bind(data, currentScoreFormat, cachedSprite);

                // 홀짝 배경색
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
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
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

        // 에디터 테스트 시 '내 순위'도 가짜로 띄워주기
        DG.Tweening.DOVirtual.DelayedCall(2.0f, () =>
        {
            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, RankDataList.Count * ItemHeight);
            ContentRect.anchoredPosition = Vector2.zero;
            LastTopIndex = -1;
            OnScroll(Vector2.zero);

            // 2초 뒤에 랭킹 리스트를 띄우면서 로딩 텍스트를 끕니다.
            StopLoadingAnimation();
        });
    }


    private void StartLoadingAnimation()
    {
        if (LoadingText == null) return;

        LoadingText.gameObject.SetActive(true);

        // 기존 애니메이션이 있다면 중복 실행 방지
        loadingDotTween?.Kill();

        // 로컬라이제이션 테이블에서 "로딩 중" 또는 "Loading" 문구를 가져옵니다.
        localizedLoadingText = LoadingBaseString.GetLocalizedString();

        // 2. 0부터 3까지 숫자를 변화시키며 점을 추가하는 DOTween 애니메이션
        int dotCount = 0;
        loadingDotTween = DOTween.To(() => dotCount, x => dotCount = x, 3, 1.5f)
            .SetLoops(-1, LoopType.Restart) // 무한 반복
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                // 점의 개수만큼 문자열을 생성합니다. (예: 1이면 ".", 3이면 "...")
                string dots = new string('.', dotCount);
                LoadingText.text = $"{localizedLoadingText}{dots}";
            });
    }

    private void StopLoadingAnimation()
    {
        // 로딩이 끝나면 애니메이션을 끄고 오버레이를 숨깁니다.
        loadingDotTween?.Kill();
        if (LoadingText != null) LoadingText.gameObject.SetActive(false);
    }
}