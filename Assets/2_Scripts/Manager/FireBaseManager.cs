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

    public void SaveScoreToDatabase(int finalScore)
    {
        if (!IsAvailable()) return;

        string uid = user.UserId;
        DatabaseReference leaderboardRef = DBReference.Child("Leaderboard").Child(uid);

        leaderboardRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[FireBaseManager] 기존 점수 조회 실패: " + task.Exception);
                return;
            }

            int existingScore = 0;
            if (task.Result.Exists && task.Result.Value != null)
                int.TryParse(task.Result.Value.ToString(), out existingScore);

            if (finalScore <= existingScore) return;

            // 1. Leaderboard — Score 숫자값 하나만 저장
            leaderboardRef.SetValueAsync(finalScore).ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError("[FireBaseManager] Leaderboard 저장 실패: " + t.Exception);
            });

            // 2. Users — Score + Name + PhotoUrl 저장
            var updates = new Dictionary<string, object>
            {
                ["Score"] = finalScore,
                ["Name"] = user.DisplayName ?? "",
                ["PhotoUrl"] = user.PhotoUrl != null ? user.PhotoUrl.ToString() : ""
            };

            DBReference.Child("Users").Child(uid)
                .UpdateChildrenAsync(updates)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted)
                        Debug.LogError("[FireBaseManager] Users 점수 저장 실패: " + t.Exception);
                    else
                        Debug.Log($"[FireBaseManager] 신기록 저장 완료: {finalScore}점");
                });
        });
    }

    public void SavePlayCount(int playCount)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(user.UserId)
            .Child("TotalPlayCount")
            .SetValueAsync(playCount)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] PlayCount 저장 실패: " + task.Exception);
                else
                    Debug.Log($"[FireBaseManager] PlayCount 저장 완료: {playCount}회");
            });
    }

    public void SaveAchievement(string skinID)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(user.UserId)
            .Child("Achievements").Child(skinID)
            .SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[FireBaseManager] 업적 저장 실패 ({skinID}): " + task.Exception);
                else
                    Debug.Log($"[FireBaseManager] 업적 저장 완료: {skinID}");
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

        DBReference.Child("Leaderboard")
            .OrderByValue()
            .LimitToLast(limit)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    tcs.SetResult(new List<RankData>());
                    return;
                }

                List<DataSnapshot> entries = new List<DataSnapshot>();
                foreach (DataSnapshot child in task.Result.Children)
                    entries.Add(child);
                entries.Reverse();
                FetchDisplayInfoAsync(entries, tcs);
            });

        return tcs.Task;
    }

    // Leaderboard로 정렬된 순위(uid, score)를 받아 Users에서 표시용 Name/PhotoUrl만 골라 채운다.
    // Users 전체(Achievements 등 포함)를 내려받지 않도록 필드 단위로만 조회해 쿼리 비용을 줄인다.
    private void FetchDisplayInfoAsync(List<DataSnapshot> entries, TaskCompletionSource<List<RankData>> tcs)
    {
        if (entries.Count == 0)
        {
            tcs.SetResult(new List<RankData>());
            return;
        }

        var nameTasks = new List<Task<DataSnapshot>>();
        var photoTasks = new List<Task<DataSnapshot>>();

        foreach (DataSnapshot entry in entries)
        {
            DatabaseReference userRef = DBReference.Child("Users").Child(entry.Key);
            nameTasks.Add(userRef.Child("Name").GetValueAsync());
            photoTasks.Add(userRef.Child("PhotoUrl").GetValueAsync());
        }

        var allTasks = new List<Task<DataSnapshot>>(nameTasks);
        allTasks.AddRange(photoTasks);

        Task.WhenAll(allTasks).ContinueWithOnMainThread(_ =>
        {
            var result = new List<RankData>();

            for (int i = 0; i < entries.Count; i++)
            {
                result.Add(new RankData
                {
                    Rank = i + 1,
                    Uid = entries[i].Key,
                    Name = nameTasks[i].IsCompletedSuccessfully ? nameTasks[i].Result.Value?.ToString() ?? "" : "",
                    Score = entries[i].Value?.ToString() ?? "0",
                    PhotoUrl = photoTasks[i].IsCompletedSuccessfully ? photoTasks[i].Result.Value?.ToString() ?? "" : ""
                });
            }

            tcs.SetResult(result);
        });
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

                int myScore = 0;
                int.TryParse(task.Result.Value?.ToString(), out myScore);

                // 2단계: 내 점수 이상인 유저 수 카운트 (초경량 노드라 부담 없음)
                DBReference.Child("Leaderboard")
                    .OrderByValue()
                    .StartAt((double)myScore)
                    .GetValueAsync()
                    .ContinueWithOnMainThread(countTask =>
                    {
                        int myRank = countTask.IsCompletedSuccessfully
                            ? (int)countTask.Result.ChildrenCount
                            : -1;

                        // 3단계: 표시용 Name/PhotoUrl은 Users에서 가져오기
                        DBReference.Child("Users").Child(myUid)
                            .GetValueAsync()
                            .ContinueWithOnMainThread(userTask =>
                            {
                                string name = "";
                                string photoUrl = "";

                                if (userTask.IsCompletedSuccessfully
                                    && userTask.Result.Exists)
                                {
                                    name = userTask.Result.Child("Name").Value?.ToString() ?? "";
                                    photoUrl = userTask.Result.HasChild("PhotoUrl")
                                        ? userTask.Result.Child("PhotoUrl").Value?.ToString() ?? ""
                                        : "";
                                }

                                tcs.SetResult(new RankData
                                {
                                    Rank = myRank,
                                    Uid = myUid,
                                    Name = name,
                                    Score = myScore.ToString(),
                                    PhotoUrl = photoUrl
                                });
                            });
                    });
            });

        return tcs.Task;
    }

    public IEnumerator LoadAndRestoreUserDataRoutine()
    {
        if (!bUseFirebase || !bIsFireBaseReady || user == null)
        {
            Debug.Log("[FireBaseManager] Firebase 미사용 또는 비로그인 — 로컬 데이터 사용.");
            yield break;
        }

        bool isDone = false;

        DBReference.Child("Users").Child(user.UserId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("[FireBaseManager] 데이터 불러오기 실패: " + task.Exception);
                    isDone = true;
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // 첫 설치 → 로컬 데이터로 첫 백업 생성
                    Debug.Log("[FireBaseManager] 클라우드 데이터 없음 → 첫 백업 생성.");
                    RestoreFirstBackup();
                    isDone = true;
                    return;
                }

                // 클라우드 데이터 파싱
                UserCloudData cloudData = ParseUserCloudData(snapshot);

                // 로컬 데이터 수집
                AchievementDatabase db = AchievementManager.Instance?.Database;
                UserCloudData local = UserCloudData.FromLocalPrefs(db);

                // 병합 후 로컬 반영
                UserCloudData merged = UserCloudData.Merge(local, cloudData);
                merged.ApplyToLocalPrefs();

                // 병합 결과를 클라우드에도 동기화 (두 기기 간 최신화)
                SyncMergedData(merged);

                Debug.Log($"[FireBaseManager] 복원 완료 — " +
                          $"BestScore: {merged.BestScore} / " +
                          $"PlayCount: {merged.TotalPlayCount}");

                isDone = true;
            });

        yield return new WaitUntil(() => isDone);
    }

    private void RestoreFirstBackup()
    {
        AchievementDatabase db = AchievementManager.Instance?.Database;
        UserCloudData local = UserCloudData.FromLocalPrefs(db);
        SyncMergedData(local);
    }

    private void SyncMergedData(UserCloudData data)
    {
        if (!IsAvailable()) return;

        string uid = user.UserId;
        var userRef = DBReference.Child("Users").Child(uid);

        var updates = new Dictionary<string, object>
        {
            ["Score"] = data.BestScore,
            ["TotalPlayCount"] = data.TotalPlayCount,
            ["BestCombo"] = data.BestCombo,
            ["Name"] = user.DisplayName ?? "",
            ["PhotoUrl"] = user.PhotoUrl != null ? user.PhotoUrl.ToString() : ""
        };

        // Merge()는 항상 local/cloud 중 더 높은 점수를 선택하므로 Leaderboard도 같은 값으로 맞춰
        // 두 기기 간 복원 후 Leaderboard와 Users.Score가 어긋나는 것을 방지한다.
        DBReference.Child("Leaderboard").Child(uid)
            .SetValueAsync(data.BestScore)
            .ContinueWithOnMainThread(lbTask =>
            {
                if (lbTask.IsFaulted)
                    Debug.LogError("[FireBaseManager] Leaderboard 동기화 실패: " + lbTask.Exception);
            });

        userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[FireBaseManager] 동기화 실패: " + task.Exception);
                return;
            }

            if (data.Achievements.Count == 0) return;

            var achievementUpdates = new Dictionary<string, object>();
            foreach (var kv in data.Achievements)
                achievementUpdates[kv.Key] = kv.Value;

            userRef.Child("Achievements")
                .UpdateChildrenAsync(achievementUpdates)
                .ContinueWithOnMainThread(achTask =>
                {
                    if (achTask.IsFaulted)
                        Debug.LogError("[FireBaseManager] 업적 동기화 실패: " + achTask.Exception);
                    else
                        Debug.Log("[FireBaseManager] 클라우드 동기화 완료!");
                });
        });
    }

    private UserCloudData ParseUserCloudData(DataSnapshot snapshot)
    {
        var data = new UserCloudData();

        if (snapshot.HasChild("Score"))
            int.TryParse(snapshot.Child("Score").Value?.ToString(), out data.BestScore);

        if (snapshot.HasChild("TotalPlayCount"))
            int.TryParse(snapshot.Child("TotalPlayCount").Value?.ToString(), out data.TotalPlayCount);

        if (snapshot.HasChild("BestCombo"))
            int.TryParse(snapshot.Child("BestCombo").Value?.ToString(), out data.BestCombo);

        if (snapshot.HasChild("Achievements"))
        {
            foreach (DataSnapshot achievement in snapshot.Child("Achievements").Children)
            {
                bool isUnlocked = false;
                bool.TryParse(achievement.Value?.ToString(), out isUnlocked);
                data.Achievements[achievement.Key] = isUnlocked;
            }
        }

        return data;
    }
    public IEnumerator GoogleLoginRoutine()
    {
        if (bUseFirebase == false)
        {
            bIsLoginSequenceFinished = true;
            yield break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            bIsLoginSequenceFinished = true;
            yield break;
        }

        bIsLoginSequenceFinished = false;
        auth = FirebaseAuth.DefaultInstance;
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

        float elapsed = 0f;
        while (!bIsLoginSequenceFinished)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= 10f)
                bIsLoginSequenceFinished = true;
            yield return null;
        }
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
            if (!task.IsCanceled && !task.IsFaulted)
            {
                user = task.Result;
                SaveUserProfile();
            }
            bIsLoginSequenceFinished = true;
        });
    }

    private void SaveUserProfile()
    {
        if (!IsAvailable()) return;

        var updates = new Dictionary<string, object>
        {
            ["Name"] = user.DisplayName ?? "",
            ["PhotoUrl"] = user.PhotoUrl != null ? user.PhotoUrl.ToString() : ""
        };

        DBReference.Child("Users").Child(user.UserId)
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] 프로필 저장 실패: " + task.Exception);
                else
                    Debug.Log($"[FireBaseManager] 프로필 저장 완료: {user.DisplayName}");
            });
    }

    public void SaveBestCombo(int bestCombo)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(user.UserId)
            .Child("BestCombo")
            .SetValueAsync(bestCombo)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] BestCombo 저장 실패: " + task.Exception);
                else
                    Debug.Log($"[FireBaseManager] BestCombo 저장 완료: {bestCombo}");
            });
    }

    private bool IsAvailable()
    {
        if (!bUseFirebase || !bIsFireBaseReady || user == null)
        {
            Debug.LogWarning("[FireBaseManager] Firebase 사용 불가 상태.");
            return false;
        }
        return true;
    }

    public string GetCurrentUserUid() => user?.UserId ?? "";
    public bool IsLoggedIn => user != null;
}
