using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 힌트 데이터를 관리하는 데이터베이스 ScriptableObject.
///
/// 사용법
///   Project 창 우클릭 → Create → Tutorial → Hint Database
///   Hints 리스트에 TutorialHintData를 등록
///   TutorialHintManager의 Database 슬롯에 연결
/// </summary>
[CreateAssetMenu(fileName = "TutorialHintDatabase", menuName = "Tutorial/Hint Database")]
public class TutorialHintDatabase : ScriptableObject
{
    [SerializeField] private List<TutorialHintData> Hints = new List<TutorialHintData>();

    // 초기화 시 Dictionary로 캐싱 — O(1) 조회
    private Dictionary<string, TutorialHintData> _hintMap;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        _hintMap = new Dictionary<string, TutorialHintData>();
        foreach (TutorialHintData hint in Hints)
        {
            if (hint == null || string.IsNullOrEmpty(hint.HintID))
            {
                Debug.LogWarning("[TutorialHintDatabase] HintID가 비어있는 항목이 있습니다.");
                continue;
            }

            if (_hintMap.ContainsKey(hint.HintID))
            {
                Debug.LogWarning($"[TutorialHintDatabase] 중복된 HintID: {hint.HintID}");
                continue;
            }

            _hintMap[hint.HintID] = hint;
        }
    }

    /// <summary>ID로 힌트 데이터를 조회합니다.</summary>
    public TutorialHintData Get(string hintID)
    {
        if (_hintMap == null) BuildMap();

        if (_hintMap.TryGetValue(hintID, out TutorialHintData data))
            return data;

        Debug.LogWarning($"[TutorialHintDatabase] 힌트를 찾을 수 없음: {hintID}");
        return null;
    }

    /// <summary>등록된 모든 힌트 ID를 반환합니다. (초기화 시 사용)</summary>
    public IEnumerable<string> GetAllIDs()
    {
        if (_hintMap == null) BuildMap();
        return _hintMap.Keys;
    }
}
