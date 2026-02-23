using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameManager GameManager;
    public EnemyManager EnemyManager;

    public void StartGameLoop()
    {
        EnemyManager.SpawnEnemy();
        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        GameManager.ChangeState(GameState.PlayerTurn);
        Debug.Log("----플레이어 턴----");
    }

    public void EndPlayerTurn()
    {
        if (GameManager.CurrentState == GameState.PlayerTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        GameManager.ChangeState(GameState.EnemyTurn);
        Debug.Log("--- 적 턴 시작 (이동 및 스폰) ---");

        yield return StartCoroutine(EnemyManager.ProcessEnemiesTurnRoutine());

        StartPlayerTurn();
    }
}
