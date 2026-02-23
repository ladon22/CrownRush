using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SwipeDirection
{ 
    None,
    Up,
    Left,
    Right,
}

public class InputController : MonoBehaviour
{
    public Action<SwipeDirection, int> OnSwipeUpdate;
    public Action<SwipeDirection, int> OnSwipeEnd;
    public Action OnSwipeCanceled;

    private Vector2 StartTouchPosition;
    private bool IsDragging = false;

    public float ShortSwipeThreshold = 50f;
    public float LongSwipeThreshold = 150f;

    void Update()
    {

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
        
        SwipeDirection Direction = SwipeDirection.None;
        float Distance = SwipeDelta.magnitude;

        int MoveCount = 0;
        if (Mathf.Abs(SwipeDelta.x) > Math.Abs(SwipeDelta.y) || SwipeDelta.y < 0)
        {
            Direction = SwipeDelta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            float DistanceX = Mathf.Abs(SwipeDelta.x);

            if (DistanceX >= LongSwipeThreshold) MoveCount = 2;
            else if (DistanceX >= ShortSwipeThreshold) MoveCount = 1;
        }
        else
        {
            if (SwipeDelta.y > 0)
            {
                Direction = SwipeDirection.Up;

                float DistanceY = SwipeDelta.y;
                if (DistanceY >= ShortSwipeThreshold) MoveCount = 1;
            }
        }


        if (MoveCount == 0)
        {
            OnSwipeCanceled?.Invoke();
            return;
        }

        if (IsEnded) OnSwipeEnd?.Invoke(Direction, MoveCount);
        else OnSwipeUpdate?.Invoke(Direction, MoveCount);
    }
}
