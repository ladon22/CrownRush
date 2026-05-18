using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    [Header("Sound Settings")]
    [SerializeField] private SfxType ClickSound = SfxType.Click;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(ClickSound);
        }
    }
}
