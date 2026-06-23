using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
/// 적 정보 패널의 행(Row) 하나.
/// PieceInfoPanel에서 EnemyManager.EnemyPrefabs를 순회하며
/// Instantiate로 동적 생성한 후 Bind()로 데이터를 채웁니다.
/// </summary>
public class PieceInfoRow : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image IconImage;
    [SerializeField] private Image RowDivider;
    [SerializeField] private LocalizeStringEvent NameText;
    [SerializeField] private LocalizeStringEvent MoveDescText;   // 이동 빈도+방식을 한 문장으로 ("직선으로 끝까지, 1턴 쉬고 이동")
    [SerializeField] private LocalizeStringEvent ScoreText;

    public void Bind(
        Sprite icon,
        string nameKey,
        string moveDescKey,
        int score,
        string scoreFormatKey,
        string tableName,
        bool isLastRow = false)
    {
        if (IconImage != null)
        {
            IconImage.enabled = icon != null;
            IconImage.sprite = icon;
            IconImage.type = Image.Type.Simple;
            IconImage.preserveAspect = true;
        }

        if (NameText != null)
            NameText.StringReference.SetReference(tableName, nameKey);

        if (MoveDescText != null)
            MoveDescText.StringReference.SetReference(tableName, moveDescKey);

        if (ScoreText != null)
        {
            ScoreText.StringReference.SetReference(tableName, scoreFormatKey);
            ScoreText.StringReference.Arguments = new object[] { score };
            ScoreText.RefreshString();
        }

        if (RowDivider != null)
            RowDivider.gameObject.SetActive(!isLastRow);
    }
}
