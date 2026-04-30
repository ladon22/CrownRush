using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Down Scale")]
    [SerializeField] private Vector3 pressedScale = new Vector3(1f, 0.9f, 1f);
    private Vector3 OriginScale;
    private bool bIsPressed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OriginScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!bIsPressed)
        {
            transform.localScale = pressedScale;
            bIsPressed = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (bIsPressed)
        {
            transform.localScale = OriginScale;
            bIsPressed = false;
        }
    }
}
