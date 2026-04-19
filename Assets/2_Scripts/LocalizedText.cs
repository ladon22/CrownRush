using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string LocalizationKey;

    private TextMeshProUGUI TMPText;

    private void Awake()
    {
        TMPText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;

        UpdateText();
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        if (TMPText == null || LocalizationManager.Instance == null) return;
        TMPText.text = LocalizationManager.Instance.GetText(LocalizationKey);
    }
}
