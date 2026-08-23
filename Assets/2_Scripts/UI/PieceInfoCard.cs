using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

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
