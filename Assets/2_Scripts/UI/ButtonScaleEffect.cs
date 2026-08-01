using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Down Scale")]
    [SerializeField] private Vector3 pressedScale = new Vector3(1f, 0.9f, 1f);
    private Vector3 OriginScale;
    private bool bIsPressed;

    private RectTransform rectTransform;
    private Graphic targetGraphic;
    private Vector4 OriginRaycastPadding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OriginScale = transform.localScale;
        rectTransform = transform as RectTransform;
        targetGraphic = GetComponent<Graphic>();
        if (targetGraphic != null)
            OriginRaycastPadding = targetGraphic.raycastPadding;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!bIsPressed)
        {
            transform.localScale = pressedScale;
            bIsPressed = true;

            // localScale로 눌림 연출을 하면 클릭 판정에 쓰이는 히트 영역도 함께 줄어들어
            // 버튼 가장자리를 누를 때 OnClick이 씹히므로, 줄어든 비율만큼 raycastPadding을
            // 음수로 보정해 눌려있는 동안에도 원래 크기의 터치 영역을 유지한다.
            if (targetGraphic != null && rectTransform != null)
            {
                Rect rect = rectTransform.rect;
                float padX = pressedScale.x > 0f ? rect.width * 0.5f * (1f / pressedScale.x - 1f) : 0f;
                float padY = pressedScale.y > 0f ? rect.height * 0.5f * (1f / pressedScale.y - 1f) : 0f;
                targetGraphic.raycastPadding = new Vector4(-padX, -padY, -padX, -padY);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (bIsPressed)
        {
            transform.localScale = OriginScale;
            bIsPressed = false;

            if (targetGraphic != null)
                targetGraphic.raycastPadding = OriginRaycastPadding;
        }
    }
}
