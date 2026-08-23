using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PieceInfoPanel : MonoBehaviour
{
    [System.Serializable]
    private struct PieceMoveVisual
    {
        public EnemyPieceType Type;
        public Sprite Diagram;
    }

    [Header("UI 연결")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private GameObject Overlay;
    [SerializeField] private PieceInfoCard CardView;
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

    private readonly List<PieceCardData> CardData = new List<PieceCardData>();
    private int CurrentIndex;
    private bool ManageGameState = true;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(bool manageGameState = true)
    {
        if (gameObject.activeInHierarchy) return;

        ManageGameState = manageGameState;

        if (Overlay != null)
            Overlay.SetActive(manageGameState);

        if (manageGameState)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PushInputLock();

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
                .SetUpdate(true);
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(Hide);
    }

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

        if (ManageGameState)
        {
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
                GameManager.Instance.InputController.PopInputLock();
        }

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Pop();
    }

    private void PopulateData()
    {
        CardData.Clear();

        if (EnemyManager.Instance == null || EnemyManager.Instance.EnemyPrefabs == null) return;

        var seenTypes = new HashSet<EnemyPieceType>();

        foreach (GameObject prefab in EnemyManager.Instance.EnemyPrefabs)
        {
            if (prefab == null) continue;

            EnemyBase enemyBase = prefab.GetComponent<EnemyBase>();
            if (enemyBase == null) continue;
            if (!seenTypes.Add(enemyBase.PieceType)) continue;

            CardData.Add(new PieceCardData
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
                CardData.Add(new PieceCardData
                {
                    Type = queenBase.PieceType,
                    Icon = GetMoveDiagram(queenBase.PieceType),
                    Score = queenBase.ScoreValue
                });
            }
        }

        if (PageIndicator != null)
            PageIndicator.Setup(CardData.Count);

        ShowIndex(0, instant: true);
    }

    public void ShowNext() => ShowIndex(CurrentIndex + 1);

    public void ShowPrev() => ShowIndex(CurrentIndex - 1);

    private void ShowIndex(int index, bool instant = false)
    {
        if (CardData.Count == 0) return;

        if (LoopPages)
            index = ((index % CardData.Count) + CardData.Count) % CardData.Count;
        else
            index = Mathf.Clamp(index, 0, CardData.Count - 1);

        if (!instant && index == CurrentIndex) return;

        CurrentIndex = index;

        if (CardView != null)
        {
            PieceCardData data = CardData[CurrentIndex];
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
            PageIndicator.SetIndex(CurrentIndex);
    }

    private Sprite GetMoveDiagram(EnemyPieceType type)
    {
        foreach (PieceMoveVisual visual in MoveVisuals)
        {
            if (visual.Type == type) return visual.Diagram;
        }
        return null;
    }

    private string GetNameKey(EnemyPieceType type)
    {
        switch (type)
        {
            case EnemyPieceType.Pawn: return "piece_name_pawn";
            case EnemyPieceType.Rook: return "piece_name_rook";
            case EnemyPieceType.Bishop: return "piece_name_bishop";
            case EnemyPieceType.Knight: return "piece_name_knight";
            case EnemyPieceType.Queen: return "piece_name_queen";
            default: return "";
        }
    }

    private string GetMoveDescKey(EnemyPieceType type)
    {
        switch (type)
        {
            case EnemyPieceType.Pawn: return "piece_move_pawn"; 
            case EnemyPieceType.Rook: return "piece_move_rook";
            case EnemyPieceType.Bishop: return "piece_move_bishop";
            case EnemyPieceType.Knight: return "piece_move_knight";
            case EnemyPieceType.Queen: return "piece_move_queen";
            default: return "";
        }
    }
}
