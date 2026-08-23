using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UpdateBestScoreUI : MonoBehaviour
{
    [Header("UI Texts")]
    public LocalizeStringEvent BestScoreTextEvent;

    private void Start()
    {
        LoadBestScore();
    }

    public void LoadBestScore()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        if (BestScoreTextEvent != null)
        {
            BestScoreTextEvent.StringReference.Arguments = new object[] { bestScore };
            BestScoreTextEvent.RefreshString();
        }
    }
}
