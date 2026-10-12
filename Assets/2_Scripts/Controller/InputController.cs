using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputController : MonoBehaviour
{
    public Action<Vector2Int> OnSwipeUpdate;
    public Action<Vector2Int> OnSwipeEnd;
    public Action OnSwipeCanceled;

    private Vector2 StartTouchPosition;
    private bool IsDragging = false;

    public float SwipeThreshold = 50f;

    private int InputLockCount = 0;
    private bool IsTurnInputBlocked = true;

    public bool IsInputLocked => InputLockCount > 0 || IsTurnInputBlocked;
    private bool IsPanelInputLocked => InputLockCount > 0;
    public void PushInputLock() { InputLockCount++; }
    public void PopInputLock() { InputLockCount = Mathf.Max(0, InputLockCount - 1); }
    public void SetTurnInputBlocked(bool IsBlocked) { IsTurnInputBlocked = IsBlocked; }

    void Update()
    {
        if (IsPanelInputLocked)
        {
            IsDragging = false;
            return;
        }

        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            StartTouchPosition = Pointer.current.position.ReadValue();
            IsDragging = true;
        }
        else if (Pointer.current.press.isPressed && IsDragging)
        {
            ProcessSwipe(Pointer.current.position.ReadValue(), false);
        }
        else if (Pointer.current.press.wasReleasedThisFrame && IsDragging)
        {
            ProcessSwipe(Pointer.current.position.ReadValue(), true);
            IsDragging = false;
        }
    }
    private void ProcessSwipe(Vector2 CurrentPosition, bool IsEnded)
    {
        Vector2 SwipeDelta = CurrentPosition - StartTouchPosition;

        if (SwipeDelta.magnitude < SwipeThreshold)
        {
            OnSwipeCanceled?.Invoke();
            return;
        }
        float Angle = Mathf.Atan2(SwipeDelta.y, SwipeDelta.x) * Mathf.Rad2Deg;
        if (Angle < 0) Angle += 360f;
        Vector2Int GridDirection = Vector2Int.zero;

        if (Angle >= 22.5f && Angle < 67.5f) GridDirection = new Vector2Int(1, 1);
        else if (Angle >= 67.5f && Angle < 112.5f) GridDirection = new Vector2Int(0, 1);
        else if (Angle >= 112.5f && Angle < 157.5f) GridDirection = new Vector2Int(-1, 1);
        else if (Angle >= 157.5f && Angle < 202.5f) GridDirection = new Vector2Int(-1, 0);
        else if (Angle >= 202.5f && Angle < 247.5f) GridDirection = new Vector2Int(-1, -1);
        else if (Angle >= 247.5f && Angle < 292.5f) GridDirection = new Vector2Int(0, -1);
        else if (Angle >= 292.5f && Angle < 337.5f) GridDirection = new Vector2Int(1, -1);
        else GridDirection = new Vector2Int(1, 0);

        if (IsEnded) OnSwipeEnd?.Invoke(GridDirection);
        else OnSwipeUpdate?.Invoke(GridDirection);
    }
}
