using UnityEngine;
using TMPro;
using DG.Tweening;

public class AchievementPopupUI : MonoBehaviour
{
    [Header("Achievement UI")]
    public GameObject AchievementPopup;
    public TextMeshProUGUI AchievementText;

    private void Awake()
    {
        if (AchievementPopup != null) AchievementPopup.SetActive(false);
    }

    // 오브젝트가 켜질 때 매니저의 방송을 구독(구독하기)
    private void OnEnable()
    {
        AchievementManager.OnAchievementUnlocked += ShowPopup;
    }

    // 오브젝트가 꺼질 때 매니저의 방송을 취소(구독취소) - 메모리 누수 방지
    private void OnDisable()
    {
        AchievementManager.OnAchievementUnlocked -= ShowPopup;
    }

    // 매니저가 방송을 하면 자동으로 실행되는 함수
    private void ShowPopup(AchievementEntry data)
    {
        if (AchievementPopup == null || AchievementText == null) return;

        // 넘어온 데이터의 제목과 보상 텍스트를 UI에 적용
        AchievementText.text = $"{data.Title}\n{data.RewardText}";

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