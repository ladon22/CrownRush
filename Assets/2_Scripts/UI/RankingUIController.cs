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

    private RectTransform ContentRect;
    private List<GameObject> PoolItems = new List<GameObject>();

    private List<RankData> RankDataList = new List<RankData>();
    private Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
    private HashSet<string> DownloadingUrls = new HashSet<string>();

    private int LastTopIndex = -1;

    [Header("Loading Animation")]
    [SerializeField] private TextMeshProUGUI LoadingText; // "로딩 중" 글자가 써질 텍스트
    [SerializeField] private LocalizedString LoadingBaseString; // "로딩 중"이라는 기본 다국어 키

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
            obj.SetActive(false);
            PoolItems.Add(obj);
        }
    }

    public void ShowRanking()
    {
        gameObject.SetActive(true);

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase).SetUpdate(true);
        }

        // 창이 열릴 때 다국어 텍스트를 딱 한 번만 가져옵니다 (최적화)
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
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 새로고침 버튼과 ShowRanking에서 공통으로 쓰는 함수
    public void RefreshRanking()
    {
        RankDataList.Clear();
        LastTopIndex = -1;
        ContentRect.anchoredPosition = Vector2.zero;
        foreach (var item in PoolItems) item.SetActive(false);

        LoadRankingData();
    }

    private void LoadRankingData()
    {
        StartLoadingAnimation();

        if (FireBaseManager.Instance != null && FireBaseManager.Instance.bUseFirebase == false)
        {
            Debug.Log("파이어베이스가 꺼져있어 테스트용 더미(Dummy) 데이터를 띄웁니다.");
            LoadDummyDataForEditor();
            return;
        }

        // 로그인된 내 정보 가져오기
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser currentUser = auth?.CurrentUser;
        string myUid = currentUser != null ? currentUser.UserId : "";

        FirebaseDatabase.DefaultInstance.RootReference.Child("Leaderboard")
            .OrderByChild("Score")
            .LimitToLast(50)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("랭킹 데이터를 불러오는데 실패했습니다.");
                    return;
                }

                RankDataList.Clear();
                List<DataSnapshot> snapshots = new List<DataSnapshot>();
                foreach (DataSnapshot child in task.Result.Children) snapshots.Add(child);
                snapshots.Reverse();

                bool bAmIInTop50 = false;

                for (int i = 0; i < snapshots.Count; i++)
                {
                    DataSnapshot data = snapshots[i];
                    RankData newRankData = new RankData
                    {
                        Rank = i + 1,
                        Name = data.Child("Name").Value.ToString(),
                        Score = data.Child("Score").Value.ToString(),
                        PhotoUrl = data.HasChild("PhotoUrl") ? data.Child("PhotoUrl").Value.ToString() : ""
                    };
                    RankDataList.Add(newRankData);

                    // 내가 50위 안에 있는지 체크
                    if (data.Key == myUid)
                    {
                        bAmIInTop50 = true;
                        UpdateMyRankUI(newRankData.Rank, newRankData.Name, newRankData.Score, newRankData.PhotoUrl);
                    }

                    if (!string.IsNullOrEmpty(newRankData.PhotoUrl))
                    {
                        if (!ImageCache.ContainsKey(newRankData.PhotoUrl) && !DownloadingUrls.Contains(newRankData.PhotoUrl))
                        {
                            StartCoroutine(LoadProfileImageRoutine(newRankData.PhotoUrl));
                        }
                    }
                }

                // 내가 50위 밖에 있다면 내 점수만 따로 검색합니다.
                if (!bAmIInTop50 && !string.IsNullOrEmpty(myUid))
                {
                    LoadMyDataOutsideTop50(myUid, currentUser);
                }
                else
                {
                    StopLoadingAnimation();
                }

                ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, RankDataList.Count * ItemHeight);
                ContentRect.anchoredPosition = Vector2.zero;
                LastTopIndex = -1;
                OnScroll(Vector2.zero);
            });
    }

    // 50위 밖 진짜 순위 계산 로직
    private void LoadMyDataOutsideTop50(string myUid, FirebaseUser user)
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child("Leaderboard").Child(myUid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot myData = task.Result;
                int myScore = int.Parse(myData.Child("Score").Value.ToString());
                string myName = myData.Child("Name").Value.ToString();
                string myPhoto = myData.HasChild("PhotoUrl") ? myData.Child("PhotoUrl").Value.ToString() : "";

                // 나보다 점수가 높은 사람이 몇 명인지 셉니다.
                FirebaseDatabase.DefaultInstance.RootReference.Child("Leaderboard")
                    .OrderByChild("Score")
                    .StartAt(myScore)
                    .GetValueAsync().ContinueWithOnMainThread(countTask =>
                    {
                        if (countTask.IsCompleted)
                        {
                            int myActualRank = (int)countTask.Result.ChildrenCount;
                            UpdateMyRankUI(myActualRank, myName, myScore.ToString(), myPhoto);
                        }
                    });
            }
            else
            {
                string myPhoto = user.PhotoUrl != null ? user.PhotoUrl.ToString() : "";
                UpdateMyRankUI(-1, user.DisplayName, "0", myPhoto);
            }

            StopLoadingAnimation();
        });
    }

    // 내 상단 UI 갱신 (점·콤마·다국어 적용)
    private void UpdateMyRankUI(int rank, string name, string score, string photoUrl)
    {
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
                PoolItems[i].SetActive(true);

                RectTransform rt = PoolItems[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -dataIndex * ItemHeight);

                RankData data = RankDataList[dataIndex];

                // 배경색 교차
                Image bgImage = PoolItems[i].GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = (dataIndex % 2 == 0) ? OddRowColor : EvenRowColor;
                }

                // 점수 콤마 & 다국어 포맷팅
                int parsedScore = 0;
                int.TryParse(data.Score, out parsedScore);
                string formattedScore = parsedScore.ToString("N0");

                PoolItems[i].transform.Find("RankText").GetComponent<TextMeshProUGUI>().text = data.Rank.ToString();
                PoolItems[i].transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = data.Name;
                PoolItems[i].transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = string.Format(currentScoreFormat, formattedScore);

                Image profileImage = PoolItems[i].transform.Find("ProfileImage").GetComponent<Image>();
                profileImage.sprite = null;

                if (!string.IsNullOrEmpty(data.PhotoUrl))
                {
                    if (ImageCache.ContainsKey(data.PhotoUrl))
                    {
                        profileImage.sprite = ImageCache[data.PhotoUrl];
                    }
                }
            }
            else
            {
                PoolItems[i].SetActive(false);
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

    // 내 순위 전용 이미지 로더
    //private IEnumerator LoadSingleImageRoutine(string url, Image targetImage)
    //{
    //    UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success && targetImage != null)
    //    {
    //        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    //        targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //        if (!ImageCache.ContainsKey(url)) ImageCache.Add(url, targetImage.sprite);
    //    }
    //}

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