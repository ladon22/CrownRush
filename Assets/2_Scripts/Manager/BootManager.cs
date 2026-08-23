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
    public Slider LoadingProgressBar;

    private class LoadingTask
    {
        public Func<IEnumerator> TaskRoutine;
    }

    private List<LoadingTask> LoadingTasks = new List<LoadingTask>();

    void Start()
    {
        if (LoadingProgressBar != null) LoadingProgressBar.value = 0f;

        RegisterTasks();
        StartCoroutine(BootSequenceRoutine());
    }

    private void RegisterTasks()
    {
        LoadingTasks.Add(new LoadingTask { TaskRoutine = InitLocalizationRoutine });

        if (FireBaseManager.Instance != null)
        {
            LoadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.InitializeFirebaseRoutine });
            LoadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.GoogleLoginRoutine });
            LoadingTasks.Add(new LoadingTask { TaskRoutine = FireBaseManager.Instance.LoadAndRestoreUserDataRoutine });
        }
    }

    private IEnumerator BootSequenceRoutine()
    {
        int totalTasks = LoadingTasks.Count;

        for (int i = 0; i < totalTasks; i++)
        {
            LoadingTask currentTask = LoadingTasks[i];

            yield return StartCoroutine(currentTask.TaskRoutine());

            float progress = (float)(i + 1) / totalTasks;
            UpdateProgress(progress);

            yield return new WaitForSeconds(1.0f);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(1);
        }
        else
        {
            SceneManager.LoadScene(1);
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
            LoadingProgressBar.DOValue(value, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
