using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public EnemyManager EnemyManager;
    public float TurnDelay = 0.5f;
    public void StartGameLoop()
    {
        EnemyManager.SpawnEnemy();
        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        StartCoroutine(PlayerTurnRoutine());
    }

    public void EndPlayerTurn()
    {
        if (GameManager.Instance.CurrentState == GameState.PlayerTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        GameManager.Instance.ChangeState(GameState.EnemyTurn);
        GameManager.Instance.InputController.SetTurnInputBlocked(true);

        if (EnemyManager != null) EnemyManager.ClearAllIntents();
        yield return new WaitForSeconds(TurnDelay);

        yield return StartCoroutine(EnemyManager.ProcessEnemiesTurnRoutine());

        if (GameManager.Instance.CurrentState == GameState.GameOver) yield break;

        if (TutorialHintManager.Instance != null)
            TutorialHintManager.Instance.TryShowHint(TutorialHintID.Intent);

        StartPlayerTurn();
    }

    private IEnumerator PlayerTurnRoutine()
    {

        GameManager.Instance.ChangeState(GameState.PlayerTurn);
        GameManager.Instance.InputController.SetTurnInputBlocked(false);

        if (EnemyManager != null) EnemyManager.UpdateEnemyIntents();

        yield return new WaitForSeconds(TurnDelay);

        if (TutorialHintManager.Instance != null)
            TutorialHintManager.Instance.TryShowHint(TutorialHintID.Swipe);
    }
}
