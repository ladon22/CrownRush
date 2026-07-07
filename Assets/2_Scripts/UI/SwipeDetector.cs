using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 가로 드래그를 감지해 좌/우 스와이프 이벤트를 발생시킵니다.
/// 화면을 실시간으로 따라 움직이지는 않고, 임계값을 넘으면 이벤트만 한 번 쏩니다.
/// 같은 GameObject에 Raycast Target이 켜진 Graphic(Image 등)이 있어야 드래그 이벤트를 받습니다.
/// </summary>
public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private float SwipeThreshold = 50f;

    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;

    private Vector2 _startPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float deltaX = eventData.position.x - _startPos.x;
        if (Mathf.Abs(deltaX) < SwipeThreshold) return;

        if (deltaX < 0) OnSwipeLeft?.Invoke();
        else OnSwipeRight?.Invoke();
    }
}
