using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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
///
///   이름/이동 방식 문구, 그리고 이동 다이어그램 이미지는 밸런스 데이터가
///   아니라 UI 전용 디자인 에셋이므로 PieceType별로 이 스크립트가
///   직접 매핑을 들고 있습니다. (EnemyPrefab에서 읽어올 수 없는 값)
///
/// 카드 캐러셀 — 기물 종류마다 Row를 나열하던 방식 대신, 프리팹 안에
///   미리 배치해 둔 카드(CardView) 하나를 재사용하며 ShowIndex()로
///   내용만 교체합니다. 좌/우 버튼과 SwipeDetector의 UnityEvent가
///   각각 ShowNext()/ShowPrev()를 호출합니다.
/// </summary>
public class PieceInfoPanel : MonoBehaviour
{
    [System.Serializable]
    private struct PieceMoveVisual
    {
        public EnemyPieceType Type;
        public Sprite Diagram; // 기물이 어떻게 이동하는지 보여주는 다이어그램 이미지
    }

    [Header("UI 연결")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private GameObject Overlay;
    [SerializeField] private PieceInfoCard CardView; // 패널 프리팹 안에 미리 배치된 카드 1개 (런타임 Instantiate 없음)
    [SerializeField] private PieceInfoPageIndicator PageIndicator;

    [Header("이동 다이어그램 이미지")]
    [SerializeField] private List<PieceMoveVisual> MoveVisuals;

    [Header("애니메이션")]
    [SerializeField] private float PopDuration = 0.4f;
    [SerializeField] private Ease PopEase = Ease.OutBack;
    [SerializeField] private float CardTransitionDuration = 0.2f;

    [Header("페이지 이동")]
    [SerializeField] private bool LoopPages = true;

    [Header("로컬라이제이션")]
    [SerializeField] private string TableName = "UI_Text";

    [SerializeField] private string ScoreFormatKey = "piece_score_format";

    private struct PieceCardData
    {
        public EnemyPieceType Type;
        public Sprite Icon;
        public int Score;
    }

    private readonly List<PieceCardData> _cardData = new List<PieceCardData>();
    private int _currentIndex;
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
        PopulateData();

        if (PanelRect != null)
        {
            PanelRect.localScale = Vector3.zero;
            PanelRect.DOKill();
            PanelRect.DOScale(Vector3.one, PopDuration)
                .SetEase(PopEase)
                .SetUpdate(true); // timeScale=0 이어도 애니메이션 동작
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(Hide);
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

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    // ── 페이지 데이터 모으기 ────────────────────────────

    private void PopulateData()
    {
        _cardData.Clear();

        if (EnemyManager.Instance == null || EnemyManager.Instance.EnemyPrefabs == null) return;

        // 같은 PieceType 프리팹이 여러 개일 수 있으므로 중복 제거
        var seenTypes = new HashSet<EnemyPieceType>();

        foreach (GameObject prefab in EnemyManager.Instance.EnemyPrefabs)
        {
            if (prefab == null) continue;

            EnemyBase enemyBase = prefab.GetComponent<EnemyBase>();
            if (enemyBase == null) continue;
            if (!seenTypes.Add(enemyBase.PieceType)) continue; // 이미 담은 타입이면 건너뜀

            // [핵심] 점수는 항상 prefab의 현재 값을 읽음 — 캐싱하지 않음
            // 아이콘은 밸런스 데이터가 아니라 디자인 에셋이므로 MoveVisuals에서 가져옴
            _cardData.Add(new PieceCardData
            {
                Type = enemyBase.PieceType,
                Icon = GetMoveDiagram(enemyBase.PieceType),
                Score = enemyBase.ScoreValue
            });
        }

        GameObject queenPrefab = EnemyManager.Instance.QueenPrefabForDisplay;
        if (queenPrefab != null)
        {
            EnemyBase queenBase = queenPrefab.GetComponent<EnemyBase>();
            if (queenBase != null && seenTypes.Add(queenBase.PieceType))
            {
                _cardData.Add(new PieceCardData
                {
                    Type = queenBase.PieceType,
                    Icon = GetMoveDiagram(queenBase.PieceType),
                    Score = queenBase.ScoreValue
                });
            }
        }

        if (PageIndicator != null)
            PageIndicator.Setup(_cardData.Count);

        ShowIndex(0, instant: true);
    }

    // ── 페이지 이동 ──────────────────────────────

    /// <summary>다음 버튼, 왼쪽 스와이프에 연결합니다.</summary>
    public void ShowNext() => ShowIndex(_currentIndex + 1);

    /// <summary>이전 버튼, 오른쪽 스와이프에 연결합니다.</summary>
    public void ShowPrev() => ShowIndex(_currentIndex - 1);

    private void ShowIndex(int index, bool instant = false)
    {
        if (_cardData.Count == 0) return;

        if (LoopPages)
            index = ((index % _cardData.Count) + _cardData.Count) % _cardData.Count;
        else
            index = Mathf.Clamp(index, 0, _cardData.Count - 1);

        if (!instant && index == _currentIndex) return;

        _currentIndex = index;

        if (CardView != null)
        {
            PieceCardData data = _cardData[_currentIndex];
            CardView.Bind(data.Icon, GetNameKey(data.Type), GetMoveDescKey(data.Type), data.Score, ScoreFormatKey, TableName);

            if (!instant)
            {
                CardView.transform.DOKill();
                CardView.transform.localScale = Vector3.one * 0.85f;
                CardView.transform.DOScale(Vector3.one, CardTransitionDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }

        if (PageIndicator != null)
            PageIndicator.SetIndex(_currentIndex);
    }

    // ── 텍스트/이미지 라벨 매핑 (밸런스 데이터 아님, UI 전용) ──────

    /// <summary>PieceType → 이동 다이어그램 스프라이트. MoveVisuals 인스펙터 리스트에서 찾습니다.</summary>
    private Sprite GetMoveDiagram(EnemyPieceType type)
    {
        foreach (PieceMoveVisual visual in MoveVisuals)
        {
            if (visual.Type == type) return visual.Diagram;
        }
        return null;
    }

    /// <summary>PieceType → 이름 로컬라이제이션 키.</summary>
    private string GetNameKey(EnemyPieceType type)
    {
        switch (type)
        {
            case EnemyPieceType.Pawn: return "piece_name_pawn";   // 폰
            case EnemyPieceType.Rook: return "piece_name_rook";   // 룩
            case EnemyPieceType.Bishop: return "piece_name_bishop"; // 비숍
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
            case EnemyPieceType.Pawn: return "piece_move_pawn";   // "앞으로 한 칸, 매턴 이동"
            case EnemyPieceType.Rook: return "piece_move_rook";   // "직선으로 끝까지, 1턴 쉬고 이동"
            case EnemyPieceType.Bishop: return "piece_move_bishop"; // "대각선으로 끝까지, 1턴 쉬고 이동"
            case EnemyPieceType.Knight: return "piece_move_knight"; // "L자로 이동, 뛰어넘기 가능"
            case EnemyPieceType.Queen: return "piece_move_queen";  // "모든 방향 끝까지, 매턴 이동"
            default: return "";
        }
    }
}
