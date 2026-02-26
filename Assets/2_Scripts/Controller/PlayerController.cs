using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public GridManager GridManager;
    [HideInInspector]
    public TurnManager TurnManager;
    [HideInInspector]
    public EnemyManager EnemyManager;
    [HideInInspector]
    public InputController InputController;
    [HideInInspector]
    public HealthComponent HealthComponent;

    public GameObject PreviewIndicator;

    public int CurrentX = 2;
    public int CurrentY = 0;
    public float MoveSpeed = 15f;
    public bool bIsMoving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HealthComponent = GetComponent<HealthComponent>();
        if (InputController != null)
        {
            InputController.OnSwipeUpdate += HandleSwipeUpdate;
            InputController.OnSwipeEnd += HandleSwipeEnd;
            InputController.OnSwipeCanceled += HidePreview;
        }
    }

    private void OnDestroy()
    {
        if (InputController != null)
        {
            InputController.OnSwipeUpdate -= HandleSwipeUpdate;
            InputController.OnSwipeEnd -= HandleSwipeEnd;
            InputController.OnSwipeCanceled -= HidePreview;
        }
    }

    private void HandleSwipeUpdate(Vector2Int Direction)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;

        if (bIsMoving) return;

        int TargetX = CurrentX + Direction.x;
        int TargetY = CurrentY + Direction.y;

        if (!GridManager.IsValidGridPosition(TargetX, TargetY))
        {
            HidePreview();
            return;
        }

        Vector2 PreviewPos = GridManager.GetWorldPosition(TargetX, TargetY);
        PreviewIndicator.transform.position = PreviewPos;
        PreviewIndicator.SetActive(true);
    }

    private void HandleSwipeEnd(Vector2Int Direction)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;
        HidePreview();

        if (bIsMoving) return;

        int TargetX = CurrentX + Direction.x;
        int TargetY = CurrentY + Direction.y;

        if (!GridManager.IsValidGridPosition(TargetX, TargetY)) return;

        StartCoroutine(PerformAction(TargetX, TargetY));
    }

    private IEnumerator PerformAction(int TargetX, int TargetY)
    {
        bIsMoving = true;
        Vector2 TargetPosition = GridManager.GetWorldPosition(TargetX, TargetY);

        EnemyBase TargetEnemy = null;
        if (EnemyManager != null)
        {
            TargetEnemy = EnemyManager.GetEnemyAtPosition(TargetX, TargetY);
        }

        while((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        if (TargetEnemy != null)
        {
            EnemyManager.RemoveEnemy(TargetEnemy);
            TargetEnemy.Die();
            //TODO: 파티클과 화면 흔들림
        }

        CurrentX = TargetX;
        CurrentY = TargetY;
        bIsMoving = false;

        if (TurnManager != null) TurnManager.EndPlayerTurn();
    }
    private void HidePreview()
    {
        if (PreviewIndicator != null)
        {
            PreviewIndicator.SetActive (false);
        }
    }

    public void ResetPlayer()
    {
        GridManager.GetPlayerStartIndex(out CurrentX, out CurrentY);
        transform.position = GridManager.GetWorldPosition(CurrentX, CurrentY);

        if (HealthComponent != null)
        {
            HealthComponent.ResetHealth();
        }
        bIsMoving = false;
    }
}
