using UnityEngine;
using DG.Tweening;
using UnityEngine.Localization.Components;
using UnityEngine.UI; 

public class AchievementPopupUI : MonoBehaviour
{
    [Header("Achievement UI")]
    public GameObject AchievementPopup;     // 알림창 배경 패널
    public Image RewardPreviewImage;

    [Header("Localization Tables")]
    // 테이블 이름 2개 선언
    public string AchievementTableName = "Achievement_Table";
    public string ItemTableName = "Item_Table";

    [Header("Localization Texts")]
    public LocalizeStringEvent TitleTextEvent; // "1,000점 달성!" 텍스트
    public LocalizeStringEvent ItemNameTextEvent; // "황금 기사 획득" 텍스트

    private void Start()
    {
        // 팝업 "내용물"만 끕니다. (이 스크립트가 붙은 오브젝트는 켜져 있어야 합니다!)
        if (AchievementPopup != null) AchievementPopup.SetActive(false);

        // 시작할 때 무조건 방송을 구독합니다.
        AchievementManager.OnAchievementUnlocked += ShowPopup;
    }

    private void OnDestroy()
    {
        AchievementManager.OnAchievementUnlocked -= ShowPopup;
    }

    private void ShowPopup(AchievementEntry data)
    {
        if (AchievementPopup == null) return;

        // 1. 다국어 키값 갱신 (자동 번역 됨)
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

        // 2. 알림창 등장 애니메이션
        AchievementPopup.SetActive(true);
        AchievementPopup.transform.localScale = Vector3.zero;
        AchievementPopup.transform.DOKill();

        // 띠링! 하고 나타났다가
        AchievementPopup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
            // 3초 뒤에
            DOVirtual.DelayedCall(3.0f, () => {
                // 스르륵 사라짐
                AchievementPopup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => AchievementPopup.SetActive(false));
            });
        });
    }
}