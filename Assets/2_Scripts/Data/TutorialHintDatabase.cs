using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialHintDatabase", menuName = "Tutorial/Hint Database")]
public class TutorialHintDatabase : ScriptableObject
{
    [SerializeField] private List<TutorialHintData> Hints = new List<TutorialHintData>();

    private Dictionary<string, TutorialHintData> HintMap;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        HintMap = new Dictionary<string, TutorialHintData>();
        foreach (TutorialHintData hint in Hints)
        {
            if (hint == null || string.IsNullOrEmpty(hint.HintID))
            {
                Debug.LogWarning("[TutorialHintDatabase] HintID가 비어있는 항목이 있습니다.");
                continue;
            }

            if (HintMap.ContainsKey(hint.HintID))
            {
                Debug.LogWarning($"[TutorialHintDatabase] 중복된 HintID: {hint.HintID}");
                continue;
            }

            HintMap[hint.HintID] = hint;
        }
    }

    public TutorialHintData Get(string hintID)
    {
        if (HintMap == null) BuildMap();

        if (HintMap.TryGetValue(hintID, out TutorialHintData data))
            return data;

        Debug.LogWarning($"[TutorialHintDatabase] 힌트를 찾을 수 없음: {hintID}");
        return null;
    }

    public IEnumerable<string> GetAllIDs()
    {
        if (HintMap == null) BuildMap();
        return HintMap.Keys;
    }
}
