using UnityEngine;

/// <summary>
/// 힌트 하나의 데이터를 담는 ScriptableObject.
///
/// 새 힌트를 추가하려면
///   Project 창 우클릭 → Create → Tutorial → Hint Data
///   HintID, TitleKey, DescKey를 채우고 TutorialHintDatabase에 등록
/// </summary>
[CreateAssetMenu(fileName = "HintData_", menuName = "Tutorial/Hint Data")]
public class TutorialHintData : ScriptableObject
{
    [Header("식별자")]
    [Tooltip("PlayerPrefs 키로도 사용됩니다. 고유한 값으로 설정하세요.")]
    public string HintID;

    [Header("로컬라이제이션 키")]
    [Tooltip("UI_Text 테이블의 키")]
    public string TitleKey;
    public string DescKey;

    [Header("일러스트 (선택)")]
    [Tooltip("기물 이동 패턴 등 이미지가 필요할 때 사용")]
    public Sprite Illustration;
}
