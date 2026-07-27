using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UpdateBestScoreUI : MonoBehaviour
{
    [Header("UI Texts")]
    public LocalizeStringEvent BestScoreTextEvent; // �ٱ��� ������ ���� ������Ʈ

    private void Start()
    {
        LoadBestScore();

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Lobby)
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
