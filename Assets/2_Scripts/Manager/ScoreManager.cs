using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore = 0;
    public TextMeshProUGUI ScoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateScore();
    }

    public void AddScore(int ScoreToAdd)
    {
        CurrentScore += ScoreToAdd;
        UpdateScore();
    }
    // Update is called once per frame

    private void UpdateScore()
    {
        if (ScoreText != null)
        {
            ScoreText.text = $"{CurrentScore}";
        }
    }
}
