using UnityEngine;

// 1. 모든 수집품의 기본이 되는 부모 클래스입니다. (직접 생성할 수 없음)
public abstract class ItemData : ScriptableObject
{
    [Header("Common Info")]
    public string ItemID;        // 예: skin_cheerleader
}
