using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankEntryView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI RankText;
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private Image ProfileImage;
    [SerializeField] private Image BackgroundImage;

    public void Bind(RankData data, string scoreFormat, Sprite cachedSprite = null)
    {
        if (RankText != null)
            RankText.text = data.Rank.ToString();

        if (NameText != null)
            NameText.text = data.Name;

        if (ScoreText != null)
        {
            int parsedScore = 0;
            int.TryParse(data.Score, out parsedScore);
            ScoreText.text = string.Format(scoreFormat, parsedScore.ToString("N0"));
        }

        if (ProfileImage != null)
            ProfileImage.sprite = cachedSprite;
    }

    public void SetBackgroundColor(Color color)
    {
        if (BackgroundImage != null)
            BackgroundImage.color = color;
    }
}
