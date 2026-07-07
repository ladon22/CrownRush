using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
/// 적 정보 패널의 카드 한 장.
/// PieceInfoPanel이 스와이프/버튼 입력에 따라 같은 인스턴스에 Bind()를 다시 호출해
/// 내용을 갱신합니다. (PieceInfoRow와 달리 한 번에 카드 1개만 존재)
/// </summary>
public class PieceInfoCard : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image IconImage;
    [SerializeField] private LocalizeStringEvent NameText;
    [SerializeField] private LocalizeStringEvent MoveDescText;
    [SerializeField] private LocalizeStringEvent ScoreText;

    public void Bind(
        Sprite icon,
        string nameKey,
        string moveDescKey,
        int score,
        string scoreFormatKey,
        string tableName)
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
    }
}
