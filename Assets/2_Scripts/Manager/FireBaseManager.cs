using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;

    private DatabaseReference DBReference;
    private bool bIsFireBaseReady = false;

    private FirebaseAuth auth;
    private FirebaseUser user;
    [HideInInspector] public bool bIsLoginSequenceFinished = false;

    public bool bUseFirebase = false;

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
        }
    }

    public IEnumerator InitializeFirebaseRoutine()
    {
        if (bUseFirebase == false)
        {
            Debug.Log("[FireBaseManager] 파이어베이스를 사용하지 않음 설정.");
            yield break; // 코루틴 즉시 종료
        }

        bool bWaitFirebase = true;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DBReference = FirebaseDatabase.DefaultInstance.RootReference;
                bIsFireBaseReady = true;
                Debug.Log("[FireBaseManager] 초기화 성공 및 연결 완료");
            }
            else
            {
                Debug.LogError($"[FireBaseManager] 초기화 실패: {task.Result}");
            }

            bWaitFirebase = false;
        });

        // 비동기 작업이 끝날 때까지 대기
        yield return new WaitUntil(() => bWaitFirebase == false);
    }

    public void SaveScoreToDatabase(int FinalScore)
    {
        if (!bUseFirebase || !bIsFireBaseReady || DBReference == null || user == null)
        {
            Debug.LogWarning("[FireBaseManager] 점수 저장 불가 상태.");
            return;
        }

        string uid = user.UserId;
        DatabaseReference userRef = DBReference.Child("Leaderboard").Child(uid);

        userRef.Child("Score").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[FireBaseManager] 점수 조회 실패: " + task.Exception);
                return;
            }
            int existingScore = 0;
            if(task.Result.Exists && task.Result.Value != null)
                int.TryParse(task.Result.Value.ToString(), out existingScore);

            if (FinalScore <= existingScore) return;

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                ["Score"] = FinalScore,
                ["Name"] = user.DisplayName ?? "",
                ["PhotoUrl"] = user.PhotoUrl != null ? user.PhotoUrl.ToString() : ""
            };

            userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask => 
            {
                if (updateTask.IsFaulted)
                    Debug.LogError("[FireBaseManager] 점수 저장 실패: " + updateTask.Exception);
                else
                    Debug.Log("[FireBaseManager] 신기록 저장 완료!");
            });
        });
    }

    public Task<List<RankData>> GetTopRankingsAsync(int limit = 50)
    {
        var tcs = new TaskCompletionSource<List<RankData>>();

        if (!bUseFirebase || !bIsFireBaseReady)
        {
            tcs.SetResult(new List<RankData>());
            return tcs.Task;
        }

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("Leaderboard")
            .OrderByChild("Score")
            .LimitToLast(limit)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("[FireBaseManager] 랭킹 조회 실패.");
                    tcs.SetResult(new List<RankData>());
                    return;
                }

                var snapshots = new List<DataSnapshot>();
                foreach (DataSnapshot child in task.Result.Children)
                    snapshots.Add(child);
                snapshots.Reverse(); // 높은 점수 순 정렬

                var result = new List<RankData>();
                for (int i = 0; i < snapshots.Count; i++)
                {
                    DataSnapshot data = snapshots[i];
                    result.Add(new RankData
                    {
                        Rank = i + 1,
                        Uid = data.Key,
                        Name = data.Child("Name").Value?.ToString() ?? "",
                        Score = data.Child("Score").Value?.ToString() ?? "0",
                        PhotoUrl = data.HasChild("PhotoUrl")
                            ? data.Child("PhotoUrl").Value?.ToString() ?? ""
                            : ""
                    });
                }

                tcs.SetResult(result);
            });

        return tcs.Task;
    }

    public Task<RankData> GetMyRankAsync()
    {
        var tcs = new TaskCompletionSource<RankData>();

        if (!bUseFirebase || !bIsFireBaseReady || user == null)
        {
            tcs.SetResult(null);
            return tcs.Task;
        }

        string myUid = user.UserId;

        DBReference.Child("Leaderboard").Child(myUid)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully || !task.Result.Exists)
                {
                    tcs.SetResult(null);
                    return;
                }

                DataSnapshot myData = task.Result;
                int myScore = 0;
                int.TryParse(myData.Child("Score").Value?.ToString(), out myScore);
                string myName = myData.Child("Name").Value?.ToString() ?? "";
                string myPhoto = myData.HasChild("PhotoUrl")
                    ? myData.Child("PhotoUrl").Value?.ToString() ?? ""
                    : "";

                // 내 점수 이상인 사람 수 = 내 순위
                DBReference.Child("Leaderboard")
                    .OrderByChild("Score")
                    .StartAt(myScore)
                    .GetValueAsync()
                    .ContinueWithOnMainThread(countTask =>
                    {
                        int myRank = countTask.IsCompletedSuccessfully
                            ? (int)countTask.Result.ChildrenCount
                            : -1;

                        tcs.SetResult(new RankData
                        {
                            Rank = myRank,
                            Uid = myUid,
                            Name = myName,
                            Score = myScore.ToString(),
                            PhotoUrl = myPhoto
                        });
                    });
            });

        return tcs.Task;
    }

    public IEnumerator GoogleLoginRoutine()
    {
        if (bUseFirebase == false)
        {
            bIsLoginSequenceFinished = true;
            yield break;
        }

        bIsLoginSequenceFinished = false; // 로그인 진행 중 상태로 설정
        auth = FirebaseAuth.DefaultInstance;

        Debug.Log("[FireBaseManager] 구글 로그인을 시도합니다...");
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

        // 로그인이 성공하든 실패하든 완전히 끝날 때까지 BootManager의 진행을 멈춤
        yield return new WaitUntil(() => bIsLoginSequenceFinished == true);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, authCode =>
            {
                LoginToFirebase(authCode);
            });
        }
        else
        {
            Debug.LogError("GPGS 로그인 실패 또는 취소됨: " + status);
            bIsLoginSequenceFinished = true; // 실패해도 로비로는 가야 하므로 시퀀스 종료
        }
    }

    private void LoginToFirebase(string authCode)
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("파이어베이스 연동 에러: " + task.Exception);
            }
            else
            {
                user = task.Result;
                Debug.Log($"파이어베이스 로그인 최종 성공! 환영합니다: {user.DisplayName}");
            }
            bIsLoginSequenceFinished = true; // 성공/실패 여부 상관없이 씬을 넘기기 위해 종료
        });
    }

    public string GetCurrentUserUid() => user?.UserId ?? "";
    public bool IsLoggedIn => user != null;
}
