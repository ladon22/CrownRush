using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;

    private DatabaseReference DBReference;
    private bool bIsFireBaseReady = false;


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
            Debug.Log("파이어 베이스를 사용하지 않아 점수를 저장하지 않습니다.");
            return;
        }
        if (!bIsFireBaseReady || DBReference == null)
        {
            Debug.LogWarning("파이어베이스가 준비 되지 않아 점수를 저장할 수 없습니다.");
            return;
        }

        string TempUserName = "Guest_" + Random.Range(1000, 9999).ToString();

        DBReference.Child("Leaderboard").Child(TempUserName).Child("Score").SetValueAsync(FinalScore).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("점수 저장 실패" + task.Exception);
            }
            else if(task.IsCompleted)
            {
                Debug.Log($"점수 저장 성공: [{TempUserName}: {FinalScore}점]");
            }
        });
    }
}
