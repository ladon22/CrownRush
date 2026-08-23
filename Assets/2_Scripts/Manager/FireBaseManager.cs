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
using UnityEngine.Serialization;

public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;

    private DatabaseReference DBReference;
    private bool IsFireBaseReady = false;

    private FirebaseAuth Auth;
    private FirebaseUser User;
    [HideInInspector] public bool IsLoginSequenceFinished = false;

    public bool UseFirebase = false;

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
        if (UseFirebase == false)
        {
            yield break;
        }

        bool waitFirebase = true;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DBReference = FirebaseDatabase.DefaultInstance.RootReference;
                IsFireBaseReady = true;
            }
            else
            {
                Debug.LogError($"[FireBaseManager] 초기화 실패: {task.Result}");
            }

            waitFirebase = false;
        });

        yield return new WaitUntil(() => waitFirebase == false);
    }

    public void SaveScoreToDatabase(int finalScore)
    {
        if (!IsAvailable()) return;

        string uid = User.UserId;
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

            leaderboardRef.SetValueAsync(finalScore).ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError("[FireBaseManager] Leaderboard 저장 실패: " + t.Exception);
            });

            var updates = new Dictionary<string, object>
            {
                ["Score"] = finalScore,
                ["Name"] = User.DisplayName ?? "",
                ["PhotoUrl"] = User.PhotoUrl != null ? User.PhotoUrl.ToString() : ""
            };

            DBReference.Child("Users").Child(uid)
                .UpdateChildrenAsync(updates)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted)
                        Debug.LogError("[FireBaseManager] Users 점수 저장 실패: " + t.Exception);
                });
        });
    }

    public void SavePlayCount(int playCount)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(User.UserId)
            .Child("TotalPlayCount")
            .SetValueAsync(playCount)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] PlayCount 저장 실패: " + task.Exception);
            });
    }

    public void SaveAchievement(string skinID)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(User.UserId)
            .Child("Achievements").Child(skinID)
            .SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[FireBaseManager] 업적 저장 실패 ({skinID}): " + task.Exception);
            });
    }

    public Task<List<RankData>> GetTopRankingsAsync(int limit = 50)
    {
        var tcs = new TaskCompletionSource<List<RankData>>();

        if (!UseFirebase || !IsFireBaseReady)
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

        if (!UseFirebase || !IsFireBaseReady || User == null)
        {
            tcs.SetResult(null);
            return tcs.Task;
        }

        string myUid = User.UserId;

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

                DBReference.Child("Leaderboard")
                    .OrderByValue()
                    .StartAt((double)myScore)
                    .GetValueAsync()
                    .ContinueWithOnMainThread(countTask =>
                    {
                        int myRank = countTask.IsCompletedSuccessfully
                            ? (int)countTask.Result.ChildrenCount
                            : -1;
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
        if (!UseFirebase || !IsFireBaseReady || User == null)
        {
            yield break;
        }

        bool isDone = false;

        DBReference.Child("Users").Child(User.UserId)
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
                    RestoreFirstBackup();
                    isDone = true;
                    return;
                }

                UserCloudData cloudData = ParseUserCloudData(snapshot);

                AchievementDatabase db = AchievementManager.Instance?.Database;
                UserCloudData local = UserCloudData.FromLocalPrefs(db);

                UserCloudData merged = UserCloudData.Merge(local, cloudData);
                merged.ApplyToLocalPrefs();

                SyncMergedData(merged);

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

        string uid = User.UserId;
        var userRef = DBReference.Child("Users").Child(uid);

        var updates = new Dictionary<string, object>
        {
            ["Score"] = data.BestScore,
            ["TotalPlayCount"] = data.TotalPlayCount,
            ["BestCombo"] = data.BestCombo,
            ["Name"] = User.DisplayName ?? "",
            ["PhotoUrl"] = User.PhotoUrl != null ? User.PhotoUrl.ToString() : ""
        };

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
        if (UseFirebase == false)
        {
            IsLoginSequenceFinished = true;
            yield break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            IsLoginSequenceFinished = true;
            yield break;
        }

        IsLoginSequenceFinished = false;
        Auth = FirebaseAuth.DefaultInstance;
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

        float elapsed = 0f;
        while (!IsLoginSequenceFinished)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= 10f)
                IsLoginSequenceFinished = true;
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
            IsLoginSequenceFinished = true;
        }
    }

    private void LoginToFirebase(string authCode)
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCanceled && !task.IsFaulted)
            {
                User = task.Result;
                SaveUserProfile();
            }
            IsLoginSequenceFinished = true;
        });
    }

    private void SaveUserProfile()
    {
        if (!IsAvailable()) return;

        var updates = new Dictionary<string, object>
        {
            ["Name"] = User.DisplayName ?? "",
            ["PhotoUrl"] = User.PhotoUrl != null ? User.PhotoUrl.ToString() : ""
        };

        DBReference.Child("Users").Child(User.UserId)
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] 프로필 저장 실패: " + task.Exception);
            });
    }

    public void SaveBestCombo(int bestCombo)
    {
        if (!IsAvailable()) return;

        DBReference.Child("Users").Child(User.UserId)
            .Child("BestCombo")
            .SetValueAsync(bestCombo)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("[FireBaseManager] BestCombo 저장 실패: " + task.Exception);
            });
    }

    private bool IsAvailable()
    {
        if (!UseFirebase || !IsFireBaseReady || User == null)
        {
            Debug.LogWarning("[FireBaseManager] Firebase 사용 불가 상태.");
            return false;
        }
        return true;
    }

    public string GetCurrentUserUid() => User?.UserId ?? "";
    public bool IsLoggedIn => User != null;
}
