using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHintManager : MonoBehaviour
{
    public static TutorialHintManager Instance;

    [Header("데이터")]
    [SerializeField] private TutorialHintDatabase Database;

    public static event Action<TutorialHintData> OnHintRequested;
    private const string PrefKeyPrefix = "Hint_";

    private Queue<TutorialHintData> HintQueue = new Queue<TutorialHintData>();
    private bool IsShowing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TryShowHint(string hintID)
    {
        if (Database == null) return;
        string prefKey = PrefKeyPrefix + hintID;
        if (PlayerPrefs.GetInt(prefKey, 0) == 1) return;

        TutorialHintData data = Database.Get(hintID);
        if (data == null) return;

        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();

        if (IsShowing)
        {
            HintQueue.Enqueue(data);
        }
        else
        {
            ShowHint(data);
        }
    }
    public void OnHintConfirmed()
    {
        IsShowing = false;

        if (HintQueue.Count > 0)
        {
            TutorialHintData next = HintQueue.Dequeue();
            ShowHint(next);
        }
    }

    private void ShowHint(TutorialHintData data)
    {
        IsShowing = true;
        OnHintRequested?.Invoke(data);
    }

    public void ResetAllHints()
    {
        if (Database == null) return;

        foreach (string id in Database.GetAllIDs())
            PlayerPrefs.DeleteKey(PrefKeyPrefix + id);

        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    [ContextMenu("모든 힌트 초기화 (에디터 전용)")]
    private void EditorResetAllHints() => ResetAllHints();
#endif
}
