/// <summary>
/// 튜토리얼 힌트 ID 상수 모음.
/// 문자열 오타를 방지하고 자동완성을 지원합니다.
///
/// 새 힌트 추가 시 여기에 상수 1줄만 추가하면 됩니다.
/// TutorialHintManager는 수정하지 않아도 됩니다.
/// </summary>
public static class TutorialHintID
{
    // ── 현재 등록된 힌트 ──────────────────────────────────
    public const string Swipe = "swipe";   // 스와이프 조작
    public const string Intent = "intent";  // 기물 이동 턴
    public const string Combo = "combo";   // 콤보 시스템
    public const string Item = "item";    // 아이템 획득
    public const string Queen = "queen";   // 퀸 승격

    // ── 새 힌트 추가 예시 ─────────────────────────────────
    // public const string Knight = "knight";  // 나이트 등장
    // public const string Dash   = "dash";    // 대시 기물 등장
}
