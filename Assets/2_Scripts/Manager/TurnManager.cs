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
        StartCoroutine(DelayStartPlayerTurn());
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
        if (EnemyManager != null) EnemyManager.ClearAllIntents();
        yield return new WaitForSeconds(TurnDelay);

        yield return StartCoroutine(EnemyManager.ProcessEnemiesTurnRoutine());
        StartPlayerTurn();
    }

    private IEnumerator DelayStartPlayerTurn()
    {
        GameManager.Instance.ChangeState(GameState.PlayerTurn);
        if (EnemyManager != null) EnemyManager.UpdateEnemyIntents();
        yield return new WaitForSeconds(TurnDelay);
    }

}
