using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FireBaseManager : MonoBehaviour
{
    private DatabaseReference DBReference;
    private bool bIsFireBaseReady = false;


    public bool bUseFirebase = false;

    void Start()
    {
        if (bUseFirebase == false)
        {
            Debug.Log("파이어 베이스를 사용하지 않습니다.");
            return;
        }

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DBReference = FirebaseDatabase.DefaultInstance.RootReference;
                bIsFireBaseReady = true;
                Debug.Log("파이어 베이스 초기화 성공, 데이터베이스 연결 완료");
            }
            else
            {
                Debug.LogError($"파이어베이스 초기화 실패: { task.Result}");
            }
        });
        
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
