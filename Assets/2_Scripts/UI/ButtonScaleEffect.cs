using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Down Scale")]
    [FormerlySerializedAs("pressedScale")]
    [SerializeField] private Vector3 PressedScale = new Vector3(1f, 0.9f, 1f);
    private Vector3 OriginScale;
    private bool IsPressed;

    private RectTransform RectTransform;
    private Graphic TargetGraphic;
    private Vector4 OriginRaycastPadding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OriginScale = transform.localScale;
        RectTransform = transform as RectTransform;
        TargetGraphic = GetComponent<Graphic>();
        if (TargetGraphic != null)
            OriginRaycastPadding = TargetGraphic.raycastPadding;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPressed)
        {
            transform.localScale = PressedScale;
            IsPressed = true;

            if (TargetGraphic != null && RectTransform != null)
            {
                Rect rect = RectTransform.rect;
                float padX = PressedScale.x > 0f ? rect.width * 0.5f * (1f / PressedScale.x - 1f) : 0f;
                float padY = PressedScale.y > 0f ? rect.height * 0.5f * (1f / PressedScale.y - 1f) : 0f;
                TargetGraphic.raycastPadding = new Vector4(-padX, -padY, -padX, -padY);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPressed)
        {
            transform.localScale = OriginScale;
            IsPressed = false;

            if (TargetGraphic != null)
                TargetGraphic.raycastPadding = OriginRaycastPadding;
        }
    }
}
