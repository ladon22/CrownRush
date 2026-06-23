using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// 적 정보 패널.
/// 로비/인게임 양쪽의 "?" 아이콘에서 직접 호출되는 독립 패널입니다.
/// (별도 일시정지 메뉴를 거치지 않으므로 이 패널이 스스로
///  Time.timeScale과 PushUIBlock/PopUIBlock을 관리합니다)
///
/// 핵심 설계 — 단일 진실 공급원(Single Source of Truth)
///   ScoreValue 값은 EnemyManager.EnemyPrefabs의 각 프리팹에 붙은
///   EnemyBase 컴포넌트에서 매번 직접 읽습니다. 따로 저장하지 않으므로
///   밸런스 수치를 두 곳에서 관리할 필요가 없습니다.
///   (EnemyBase 프리팹의 ScoreValue만 바꾸면 자동으로 반영됨)
///
///   이름/이동 방식 문구는 PieceType별로 고정된 텍스트이므로
///   여기서 매핑합니다. (밸런스 데이터가 아니라 UI 텍스트이므로
///   중복 관리 문제가 없음)
/// </summary>
public class PieceInfoPanel : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private GameObject Overlay;
    [SerializeField] private Transform ListContainer;
    [SerializeField] private PieceInfoRow RowPrefab;

    [Header("애니메이션")]
    [SerializeField] private float PopDuration = 0.4f;
    [SerializeField] private Ease PopEase = Ease.OutBack;

    [Header("로컬라이제이션")]
    [SerializeField] private string TableName = "UI_Text";
    [SerializeField] private LocalizedString ScoreFormatString; // 예: "{0}점" / "{0} pts"

    [SerializeField] private string ScoreFormatKey = "piece_score_format";
    private readonly List<PieceInfoRow> _spawnedRows = new List<PieceInfoRow>();
    private bool bManageGameState = true;
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>"?" 아이콘 OnClick에 연결합니다. (로비/인게임 공용)</summary>
    public void Show(bool manageGameState = true)
    {
        if (gameObject.activeInHierarchy) return;

        bManageGameState = manageGameState;

        if (Overlay != null)
            Overlay.SetActive(manageGameState);

        if (manageGameState)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PushUIBlock();

            Time.timeScale = 0f;
        }


        gameObject.SetActive(true);
        PopulateList();

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration)
                .SetEase(PopEase)
                .SetUpdate(true); // timeScale=0 이어도 애니메이션 동작
        }
    }

    /// <summary>닫기 버튼에 연결합니다.</summary>
    public void Hide()
    {
        if (PanelRect != null)
        {
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(ClosePanel);
        }
        else
        {
            ClosePanel();
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);

        if (bManageGameState)
        {
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PopUIBlock();
        }
    }

    // ── 리스트 채우기 (동적 생성) ────────────────────────────

    private void PopulateList()
    {
        ClearRows();

        if (EnemyManager.Instance == null || EnemyManager.Instance.EnemyPrefabs == null) return;

        // 같은 PieceType 프리팹이 여러 개일 수 있으므로 중복 제거
        var seenTypes = new HashSet<EnemyPieceType>();

        foreach (GameObject prefab in EnemyManager.Instance.EnemyPrefabs)
        {
            if (prefab == null) continue;

            EnemyBase enemyBase = prefab.GetComponent<EnemyBase>();
            if (enemyBase == null) continue;
            if (!seenTypes.Add(enemyBase.PieceType)) continue; // 이미 표시한 타입이면 건너뜀

            // [핵심] 항상 prefab의 현재 값을 읽음 — 캐싱하지 않음
            int score = enemyBase.ScoreValue;
            Sprite icon = prefab.GetComponent<SpriteRenderer>()?.sprite;

            CreateRow(enemyBase.PieceType, icon, score);
        }

        GameObject queenPrefab = EnemyManager.Instance.QueenPrefabForDisplay;
        if (queenPrefab != null)
        {
            EnemyBase queenBase = queenPrefab.GetComponent<EnemyBase>();
            if (queenBase != null && seenTypes.Add(queenBase.PieceType))
            {
                int score = queenBase.ScoreValue;
                Sprite icon = queenPrefab.GetComponent<SpriteRenderer>()?.sprite;

                CreateRow(queenBase.PieceType, icon, score, true);
            }
        }
    }

    private void CreateRow(EnemyPieceType pieceType, Sprite icon, int score, bool isLastRow = false)
    {
        if (RowPrefab == null || ListContainer == null) return;

        PieceInfoRow row = Instantiate(RowPrefab, ListContainer);
        row.Bind(icon, GetNameKey(pieceType), GetMoveDescKey(pieceType), score, ScoreFormatKey, TableName, isLastRow);
        _spawnedRows.Add(row);
    }

    private void ClearRows()
    {
        foreach (PieceInfoRow row in _spawnedRows)
        {
            if (row != null) Destroy(row.gameObject);
        }
        _spawnedRows.Clear();
    }

    // ── 텍스트 라벨 매핑 (밸런스 데이터 아님, UI 텍스트만) ──────

    /// <summary>
    /// PieceType → 이름 로컬라이제이션 키.
    /// EnemyPieceType enum이 이동 패턴 기준 이름(Linear, Dash 등)이라
    /// 체스 기물 이름과 매핑이 필요합니다.
    /// </summary>
    private string GetNameKey(EnemyPieceType type)
    {
        switch (type)
        {
            case EnemyPieceType.Linear: return "piece_name_pawn";   // 폰
            case EnemyPieceType.Dash: return "piece_name_rook";   // 룩
            case EnemyPieceType.Diagonal: return "piece_name_bishop"; // 비숍
            case EnemyPieceType.Knight: return "piece_name_knight"; // 나이트
            case EnemyPieceType.Queen: return "piece_name_queen";  // 퀸
            default: return "";
        }
    }

    /// <summary>
    /// PieceType → 이동 방식 설명 키.
    /// 이동 빈도(매턴/1턴쉼)와 방향(직선/대각선/L자 등)을
    /// 한 문장으로 합쳐서 보여줍니다.
    ///
    /// 주의: 이 문구는 텍스트로 고정되어 있으므로, 만약 나중에
    /// 특정 기물의 MoveInterval(이동 빈도)을 밸런스 조정으로 바꾼다면
    /// 이 문구도 함께 확인해서 수정해야 합니다. (점수처럼 자동 동기화되지 않음)
    /// </summary>
    private string GetMoveDescKey(EnemyPieceType type)
    {
        switch (type)
        {
            case EnemyPieceType.Linear: return "piece_move_pawn";   // "앞으로 한 칸, 매턴 이동"
            case EnemyPieceType.Dash: return "piece_move_rook";   // "직선으로 끝까지, 1턴 쉬고 이동"
            case EnemyPieceType.Diagonal: return "piece_move_bishop"; // "대각선으로 끝까지, 1턴 쉬고 이동"
            case EnemyPieceType.Knight: return "piece_move_knight"; // "L자로 이동, 뛰어넘기 가능"
            case EnemyPieceType.Queen: return "piece_move_queen";  // "모든 방향 끝까지, 매턴 이동"
            default: return "";
        }
    }
}
