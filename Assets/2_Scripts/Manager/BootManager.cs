using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    [Header("Loading UI")]
    public Slider LoadingProgressBar; // 오직 게이지바 하나만 사용합니다.

    private class LoadingTask
    {
        public Func<IEnumerator> TaskRoutine;
    }

    private List<LoadingTask> loadingTasks = new List<LoadingTask>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LoadingProgressBar != null) LoadingProgressBar.value = 0f;

        RegisterTasks();
        StartCoroutine(BootSequenceRoutine());
    }

    private void RegisterTasks()
    {
        // 텍스트 키값 없이, '무엇을 실행할지'만 등록합니다.
        loadingTasks.Add(new LoadingTask { TaskRoutine = InitLocalizationRoutine });

        if (FireBaseManager.Instance != null)
        {
            loadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.InitializeFirebaseRoutine });
            loadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.GoogleLoginRoutine });
            loadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.LoadAndRestoreUserDataRoutine });
        }
    }

    private IEnumerator BootSequenceRoutine()
    {
        int totalTasks = loadingTasks.Count;

        for (int i = 0; i < totalTasks; i++)
        {
            LoadingTask currentTask = loadingTasks[i];

            // 1. 실제 작업(코루틴)을 실행하고 기다림
            yield return StartCoroutine(currentTask.TaskRoutine());

            // 2. 퍼센트 갱신 (완료된 작업 / 전체 작업)
            float progress = (float)(i + 1) / totalTasks;
            UpdateProgress(progress);

            // 게이지 차오르는 것을 살짝 보여줄 대기 시간
            yield return new WaitForSeconds(1.0f);
        }

        // 수정: 씬을 바로 넘기지 않고 페이드 아웃 연출을 실행합니다.
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(1);
        }
        else
        {
            SceneManager.LoadScene(1); // 매니저가 없을 때를 대비한 방어 코드
        }
    }

    private IEnumerator InitLocalizationRoutine()
    {
        yield return LocalizationSettings.InitializationOperation;
    }

    private void UpdateProgress(float value)
    {
        if (LoadingProgressBar != null)
        {
            // 부드럽게 게이지 상승
            LoadingProgressBar.DOValue(value, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
