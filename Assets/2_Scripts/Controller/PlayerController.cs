using System;
using System.Collections;
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
    public bool IsMoving = false;
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

    private void HandleSwipeUpdate(SwipeDirection Direction, int MoveCount)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;

        if (IsMoving) return;

        int TargetX = CurrentX;
        int TargetY = CurrentY;
        if (Direction == SwipeDirection.Left) TargetX -= MoveCount;
        else if (Direction == SwipeDirection.Right) TargetX += MoveCount;

        TargetX = Mathf.Clamp(TargetX, 0, GridManager.Columns - 1);

        if (Direction == SwipeDirection.Up)  
        {
            TargetX = CurrentX;
            TargetY += MoveCount;
        }
        TargetY = Mathf.Clamp(TargetY, 0, GridManager.Rows - 1);

        Vector2 PreviewPos = GridManager.GetWorldPosition(TargetX, TargetY);
        PreviewIndicator.transform.position = PreviewPos;
        PreviewIndicator.SetActive(true);
    }

    private void HandleSwipeEnd(SwipeDirection Direction, int MoveCount)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;
        HidePreview();

        if (IsMoving) return;

        int TargetX = CurrentX;

        if (Direction == SwipeDirection.Left) TargetX -= MoveCount;
        else if (Direction == SwipeDirection.Right) TargetX += MoveCount;

        TargetX = Mathf.Clamp(TargetX, 0, GridManager.Columns - 1);
        if (Direction == SwipeDirection.Up)
        {
            Attack();
        }
        else if (TargetX != CurrentX)
        {
            CurrentX = TargetX;
            Vector2 TargetWorldPos = GridManager.GetWorldPosition(CurrentX, CurrentY);
            StartCoroutine(SmoothMove(TargetWorldPos));
        }
    }

    private void HidePreview()
    {
        if (PreviewIndicator != null)
        {
            PreviewIndicator.SetActive (false);
        }
    }
    private void Attack()
    {
        Debug.Log("АјАн");
        StartCoroutine(SmoothAttack());
    }

    private IEnumerator SmoothMove(Vector2 TargetPosition)
    {
        IsMoving = true;
        while((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        IsMoving = false;

        AttackEnemyAt(CurrentX, CurrentY);
        if (TurnManager != null) TurnManager.EndPlayerTurn();
    }

    private IEnumerator SmoothAttack()
    {
        IsMoving = true;
        Vector2 TargetPosition = GridManager.GetWorldPosition(CurrentX, CurrentY + 1);
        while ((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        AttackEnemyAt(CurrentX, CurrentY + 1);

        TargetPosition = GridManager.GetWorldPosition(CurrentX,CurrentY);
        while ((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        IsMoving = false;
        if (TurnManager != null) TurnManager.EndPlayerTurn();
    }

    private void AttackEnemyAt(int TargetX, int TargetY)
    {
        if (EnemyManager != null)
        {
            EnemyBase SelectedEnemy = EnemyManager.GetEnemyAtPosition(TargetX, TargetY);
            if (SelectedEnemy != null)
            {
                EnemyManager.RemoveEnemy(SelectedEnemy);
                SelectedEnemy.Die();
            }
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
        IsMoving = false;
    }
}
