using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 힌트 관리자.
///
/// TutorialHintPanel을 직접 참조하지 않습니다.
/// 힌트 표시가 필요할 때 OnHintRequested 이벤트를 발행하고
/// TutorialHintPanel이 이벤트를 구독해서 스스로 표시합니다.
///
/// 호출 예시
///   TutorialHintManager.Instance.TryShowHint(TutorialHintID.Combo);
/// </summary>
public class TutorialHintManager : MonoBehaviour
{
    public static TutorialHintManager Instance;

    [Header("데이터")]
    [SerializeField] private TutorialHintDatabase Database;

    // ── 이벤트 ───────────────────────────────────────────────
    // TutorialHintPanel이 구독해서 표시 처리를 담당합니다.
    // Manager는 누가 구독하는지 알 필요가 없습니다.
    public static event Action<TutorialHintData> OnHintRequested;
    private const string PrefKeyPrefix = "Hint_";

    private Queue<TutorialHintData> _hintQueue = new Queue<TutorialHintData>();
    private bool _isShowing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── 핵심 API ─────────────────────────────────────────────

    /// <summary>
    /// 힌트를 최초 1회만 표시합니다.
    /// 이미 표시한 힌트는 무시됩니다.
    /// </summary>
    public void TryShowHint(string hintID)
    {
        if (Database == null) return;

        // 이미 표시한 힌트면 건너뜀
        string prefKey = PrefKeyPrefix + hintID;
        if (PlayerPrefs.GetInt(prefKey, 0) == 1) return;

        // 데이터 조회
        TutorialHintData data = Database.Get(hintID);
        if (data == null) return;

        // 표시 기록 저장
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();

        if (_isShowing)
        {
            // 현재 힌트 표시 중 → 대기열에 추가
            _hintQueue.Enqueue(data);
            Debug.Log($"[TutorialHintManager] 힌트 대기열 추가: {hintID} (대기 {_hintQueue.Count}개)");
        }
        else
        {
            // 표시 중인 힌트 없음 → 바로 표시
            ShowHint(data);
        }
    }
    public void OnHintConfirmed()
    {
        _isShowing = false;

        if (_hintQueue.Count > 0)
        {
            TutorialHintData next = _hintQueue.Dequeue();
            ShowHint(next);
            Debug.Log($"[TutorialHintManager] 다음 힌트 표시 (남은 대기 {_hintQueue.Count}개)");
        }
    }

    private void ShowHint(TutorialHintData data)
    {
        _isShowing = true;
        OnHintRequested?.Invoke(data);
        Debug.Log($"[TutorialHintManager] 힌트 표시: {data.HintID}");
    }
    // ── 유틸 ─────────────────────────────────────────────────

    /// <summary>특정 힌트의 표시 기록을 초기화합니다.</summary>
    public void ResetHint(string hintID)
    {
        PlayerPrefs.DeleteKey(PrefKeyPrefix + hintID);
        PlayerPrefs.Save();
    }

    /// <summary>모든 힌트의 표시 기록을 초기화합니다.</summary>
    public void ResetAllHints()
    {
        if (Database == null) return;

        foreach (string id in Database.GetAllIDs())
            PlayerPrefs.DeleteKey(PrefKeyPrefix + id);

        PlayerPrefs.Save();
        Debug.Log("[TutorialHintManager] 모든 힌트 기록 초기화 완료.");
    }

#if UNITY_EDITOR
    [ContextMenu("모든 힌트 초기화 (에디터 전용)")]
    private void EditorResetAllHints() => ResetAllHints();
#endif
}
