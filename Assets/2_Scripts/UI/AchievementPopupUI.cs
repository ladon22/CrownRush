using UnityEngine;
using DG.Tweening;
using UnityEngine.Localization.Components;
using UnityEngine.UI; 

public class AchievementPopupUI : MonoBehaviour
{
    [Header("Achievement UI")]
    public GameObject AchievementPopup;
    public Image RewardPreviewImage;

    [Header("Localization Tables")]

    public string AchievementTableName = "Achievement_Table";
    public string ItemTableName = "Item_Table";

    [Header("Localization Texts")]
    public LocalizeStringEvent TitleTextEvent;
    public LocalizeStringEvent ItemNameTextEvent;

    private void Start()
    {
        if (AchievementPopup != null) AchievementPopup.SetActive(false);
        AchievementManager.OnAchievementUnlocked += ShowPopup;
    }

    private void OnDestroy()
    {
        AchievementManager.OnAchievementUnlocked -= ShowPopup;
    }

    private void ShowPopup(AchievementEntry data)
    {
        if (AchievementPopup == null) return;

        if (TitleTextEvent != null)
            TitleTextEvent.StringReference.SetReference(AchievementTableName, data.TitleKey);

        if (ItemNameTextEvent != null)
            ItemNameTextEvent.StringReference.SetReference(ItemTableName, data.RewardItem.ItemID);

        if (RewardPreviewImage != null && data.RewardItem is SkinData skinData)
        {
            if (skinData.PlayerSprite != null) 
            {
                RewardPreviewImage.sprite = skinData.PlayerSprite;
            }
            RewardPreviewImage.color = skinData.PlayerColor;
            RewardPreviewImage.preserveAspect = true;
        }

        AchievementPopup.SetActive(true);
        AchievementPopup.transform.localScale = Vector3.zero;
        AchievementPopup.transform.DOKill();

        AchievementPopup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
            DOVirtual.DelayedCall(3.0f, () => {
                AchievementPopup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => AchievementPopup.SetActive(false));
            });
        });
    }
}