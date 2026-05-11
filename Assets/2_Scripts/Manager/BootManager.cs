using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BootSequenceRoutine());
    }

    private IEnumerator BootSequenceRoutine()
    {
        Debug.Log("[BootManager] 부트 시퀀스 시작...");

        // 1단계: 유니티 다국어 시스템 초기화 대기
        yield return LocalizationSettings.InitializationOperation;
        Debug.Log("[BootManager] 1/2 다국어 시스템 준비 완료");

        // 2단계: 파이어베이스 시스템 초기화 대기 (FireBaseManager에게 지시)
        if (FireBaseManager.Instance != null)
        {
            // 2단계: 파이어베이스 시스템 초기화 대기
            yield return StartCoroutine(FireBaseManager.Instance.InitializeFirebaseRoutine());
            Debug.Log("[BootManager] 2/3 파이어베이스 준비 완료");

            // 3단계: 구글 로그인 완료 대기 (성공하든 취소하든 끝날 때까지 기다림)
            yield return StartCoroutine(FireBaseManager.Instance.GoogleLoginRoutine());
            Debug.Log("[BootManager] 3/3 구글 로그인 시퀀스 완료");
        }

        // 나중에 광고, 사운드, 로그인 등 새로운 매니저가 생기면 여기에 3단계, 4단계로 계속 추가만 하면 됩니다!

        Debug.Log("[BootManager] 모든 시스템 초기화 완료. 로비 씬으로 이동합니다.");

        // 최종 단계: 로비 씬으로 이동
        SceneManager.LoadScene(1);
    }
}
