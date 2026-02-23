using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public GridManager GridManager;
    [HideInInspector]
    public InputController InputController;

    public GameObject PreviewIndicator;

    public int CurrentX = 2;
    public int CurrentY = 0;
    public float MoveSpeed = 15f;
    public bool IsMoving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(InputController != null)
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
        if (IsMoving) return;

        int TargetX = CurrentX;
        if (Direction == SwipeDirection.Left) TargetX -= MoveCount;
        else if (Direction == SwipeDirection.Right) TargetX += MoveCount;

        TargetX = Mathf.Clamp(TargetX, 0, GridManager.Columns - 1);

        if (Direction == SwipeDirection.Up)  
        {
            TargetX = CurrentX;
        }

        Vector2 PreviewPos = GridManager.GetWorldPosition(TargetX, CurrentY);
        PreviewIndicator.transform.position = PreviewPos;
        PreviewIndicator.SetActive(true);
    }

    private void HandleSwipeEnd(SwipeDirection Direction, int MoveCount)
    {
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
    }

    private void UpdatePositionInstantly()
    {
        if (GridManager != null) transform.position = GridManager.GetWorldPosition(CurrentX, CurrentY);
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
    }
}
