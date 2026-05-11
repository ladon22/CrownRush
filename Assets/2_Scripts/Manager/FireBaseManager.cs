using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            return;
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
        if (bUseFirebase == false)
        {
            Debug.Log("파이어베이스를 사용하지 않아 점수를 저장하지 않습니다.");
            return;
        }
        if (!bIsFireBaseReady || DBReference == null)
        {
            Debug.LogWarning("파이어베이스가 준비되지 않아 점수를 저장할 수 없습니다.");
            return;
        }

        // 추가됨: 로그인된 유저가 있는지 확인
        if (user == null)
        {
            Debug.LogWarning("유저 정보가 없습니다. 구글 로그인이 정상적으로 완료되었는지 확인하세요.");
            return;
        }

        string currentUserId = user.UserId;       // 구글에서 발급한 고유 ID
        string currentUserName = user.DisplayName; // 구글 닉네임 (랭킹 표시용)

        string currentPhotoUrl = user.PhotoUrl != null ? user.PhotoUrl.ToString() : "";

        // 1. 해당 유저의 기존 점수를 서버에서 먼저 읽어옵니다. (비동기 처리)
        DBReference.Child("Leaderboard").Child(currentUserId).Child("Score").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("기존 점수를 불러오는데 실패했습니다: " + task.Exception);
                return;
            }

            int existingScore = 0;
            DataSnapshot snapshot = task.Result;

            // 서버에 이미 저장된 기록이 있다면 값을 가져옵니다.
            if (snapshot.Exists && snapshot.Value != null)
            {
                existingScore = int.Parse(snapshot.Value.ToString());
            }

            // 2. 이번 판 점수(FinalScore)가 서버 점수(existingScore)보다 높을 때만 덮어씁니다!
            if (FinalScore > existingScore)
            {
                DBReference.Child("Leaderboard").Child(currentUserId).Child("Score").SetValueAsync(FinalScore);
                DBReference.Child("Leaderboard").Child(currentUserId).Child("Name").SetValueAsync(currentUserName);

                //프로필 사진 URL도 같이 저장!
                DBReference.Child("Leaderboard").Child(currentUserId).Child("PhotoUrl").SetValueAsync(currentPhotoUrl);

                Debug.Log("서버 점수 갱신 완료!");
            }
        });
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
}
