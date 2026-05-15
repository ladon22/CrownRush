using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[System.Serializable]
public class PreviewSlotUI
{
    public Image TileImage;
    public Image PieceImage;
}

public class CollectionUIController : MonoBehaviour
{
    [Header("Item Preview UI - Full Set")]
    public PreviewSlotUI[] PreviewSlots;

    [Header("Panel Animation")]
    public RectTransform PanelRect;

    [Header("Localization Tables")]
    // 테이블을 용도별로 나누어 이름을 받습니다.
    public string UITableName = "UI_Text";
    public string ItemTableName = "Item_Table";
    public string AchievementTableName = "Achievement_Table";

    [Header("Item Preview UI")]
    // 기존 TextMeshProUGUI 대신 LocalizeStringEvent를 사용합니다.
    public LocalizeStringEvent ItemNameTextEvent;

    [Header("Unlock Status UI (잠금/진행도 박스)")]
    public GameObject LockOverlayPanel;

    public LocalizeStringEvent UnlockConditionTextEvent;
    public TextMeshProUGUI ProgressText;
    public Slider ProgressSlider;

    [Header("Navigation & Action")]
    public Button LeftButton;
    public Button RightButton;
    public Button SelectButton;
    public LocalizeStringEvent SelectTextEvent;

    [Header("Item Preview UI - Frame")]
    public Image PreviewFrameImage;

    // 데이터베이스에서 스킨만 따로 모아둘 리스트
    private List<AchievementEntry> SkinList = new List<AchievementEntry>();
    private int CurrentIndex = 0;
    private Sprite defaultPreviewFrameSprite;

    private void Awake()
    {
        // 시작할 때 원래 UI에 적용되어 있던 기본 이미지를 저장해둡니다.
        if (PreviewFrameImage != null)
        {
            defaultPreviewFrameSprite = PreviewFrameImage.sprite;
        }
    }
    private void Start()
    {
        // 버튼 이벤트 연결
        if (LeftButton != null) LeftButton.onClick.AddListener(ShowPrevious);
        if (RightButton != null) RightButton.onClick.AddListener(ShowNext);
        if (SelectButton != null) SelectButton.onClick.AddListener(EquipCurrentItem);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // 1. 데이터베이스에서 스킨 목록만 가져와서 리스트 세팅
        SkinList.Clear();
        if (AchievementManager.Instance != null && AchievementManager.Instance.Database != null)
        {
            foreach (var data in AchievementManager.Instance.Database.Achievements)
            {
                if (data.RewardItem is SkinData)
                {
                    SkinList.Add(data);
                }
            }
        }

        string equippedID = PlayerPrefs.GetString("Equipped_Skin", "skin_default");
        CurrentIndex = 0; // 혹시 못 찾을 경우를 대비한 기본값

        //3. [추가] 리스트를 훑어보며 장착 중인 스킨이 몇 번째에 있는지 찾습니다.
        for (int i = 0; i < SkinList.Count; i++)
        {
            if (SkinList[i].RewardItem != null && SkinList[i].RewardItem.ItemID == equippedID)
            {
                CurrentIndex = i;
                break;
            }
        }

        UpdateUI();

        // 팝업 애니메이션
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 화면(글자, 게이지, 버튼 상태)을 갱신하는 핵심 함수
    private void UpdateUI()
    {
        if (SkinList.Count == 0) return;

        AchievementEntry currentData = SkinList[CurrentIndex];

        bool isDefaultSkin = (currentData.RewardItem.ItemID == "skin_default");



        // 이름 갱신
        if (ItemNameTextEvent != null && currentData.RewardItem != null)
        {
            ItemNameTextEvent.StringReference.SetReference(ItemTableName, currentData.RewardItem.ItemID);
        }

        if (currentData.RewardItem is SkinData skinData) // (기존 로직 유지)
        {
            UpdateSkinDiorama(skinData);

            if (PreviewFrameImage != null)
            {
                if (skinData.FrameSprite != null)
                {
                    PreviewFrameImage.sprite = skinData.FrameSprite;
                }
                else
                {
                    PreviewFrameImage.sprite = defaultPreviewFrameSprite;
                }
                PreviewFrameImage.type = Image.Type.Sliced;
                PreviewFrameImage.pixelsPerUnitMultiplier = 0.65f;
                PreviewFrameImage.color = skinData.FrameColor;
            }
        }
        else
        {
            // 스킨이 아닌 일반 아이템일 경우의 처리 (필요하다면)
            ClearPreviewSlots();
        }

        int currentProgress = GetCurrentProgress(currentData.Type);
        // 해금 여부 및 장착 여부 확인
        string unlockKey = $"{currentData.RewardItem.ItemID}_Unlocked";
        // RewardID가 "Skin_Default" (또는 "Board_Default")인 기본 아이템은 무조건 true(해금)로 처리합니다!
        bool isUnlocked = isDefaultSkin || (PlayerPrefs.GetInt(unlockKey, 0) == 1);

        if (!isUnlocked && currentProgress >= currentData.TargetValue)
        {
            isUnlocked = true;
            PlayerPrefs.SetInt(unlockKey, 1);
            PlayerPrefs.Save();
        }

        string equippedID = PlayerPrefs.GetString("Equipped_Skin", "skin_default");
        bool isEquipped = (equippedID == currentData.RewardItem.ItemID);

        if (isDefaultSkin)
        {
            if (LockOverlayPanel != null) LockOverlayPanel.SetActive(false);
        }
        else
        {
            // 기본 스킨이 아닐 때만 텍스트와 게이지를 갱신합니다.
            if (UnlockConditionTextEvent != null)
                UnlockConditionTextEvent.StringReference.SetReference(AchievementTableName, currentData.TitleKey);

            if (ProgressText != null)
                ProgressText.text = $"{(currentProgress < currentData.TargetValue ? currentProgress : currentData.TargetValue)} / {currentData.TargetValue}";

            if (ProgressSlider != null)
                ProgressSlider.value = Mathf.Clamp01((float)currentProgress / currentData.TargetValue);

            // 해금 여부와 상관없이 오버레이 패널(조건 텍스트 등 포함)은 일단 켜둡니다! 
            // (그래야 "조건 달성", "열린 자물쇠" 등이 보이니까요)
            if (LockOverlayPanel != null) LockOverlayPanel.SetActive(true);
        }

        if (isUnlocked)
        {
            SelectButton.interactable = !isEquipped; // 이미 장착 중이면 버튼 비활성화
            // [복구] 버튼의 텍스트를 현재 상태에 맞게 바꿔줍니다!
            if (SelectTextEvent != null)
            {
                SelectTextEvent.StringReference.SetReference(UITableName, isEquipped ? "ui_btn_equipped" : "ui_btn_select");
            }
        }
        else
        {
            // 잠긴 상태 (이미지와 똑같이 진행도 바 표시)
            if (LockOverlayPanel != null) LockOverlayPanel.SetActive(true);
            SelectButton.interactable = false;

            if (SelectTextEvent != null)
                SelectTextEvent.StringReference.SetReference(UITableName, "ui_btn_locked");
        }
    }

    private void UpdateSkinDiorama(SkinData skinData)
    {
        // 인스펙터 세팅이 누락되었는지 안전 검사
        if (PreviewSlots == null || PreviewSlots.Length < 6)
        {
            Debug.LogWarning("CollectionUI: PreviewSlots 배열의 크기가 6이 아닙니다! 인스펙터를 확인해주세요.");
            return;
        }

        //  3x2 체스판 패턴에 맞게 교차되는 타일 색상 배열 (Light, Dark 교대)
        Sprite[] tiles = {
            skinData.LightTileSprite, skinData.DarkTileSprite, skinData.LightTileSprite,
            skinData.DarkTileSprite, skinData.LightTileSprite, skinData.DarkTileSprite
        };

        // 1. 첫 번째 슬롯 [0] : 플레이어 (King)
        SetSlotUI(PreviewSlots[0], tiles[0], skinData.PlayerSprite, skinData.PlayerColor);

        // 2. 나머지 슬롯 [1]~[5] : 적 기물들
        // Enum 순서를 UI에 보여주고 싶은 순서대로 배치합니다.
        EnemyPieceType[] enemyTypes = {
            EnemyPieceType.Linear,   // [1] 폰
            EnemyPieceType.Knight,   // [2] 나이트
            EnemyPieceType.Dash,     // [3] 룩
            EnemyPieceType.Diagonal, // [4] 비숍
            EnemyPieceType.Queen     // [5] 퀸
        };

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            // 스킨 데이터에서 해당 타입의 적 스프라이트를 찾아옵니다.
            EnemySkinData enemySkin = skinData.GetEnemySkin(enemyTypes[i]);

            // 플레이어가 0번이므로 적은 1번 인덱스부터 시작
            int slotIndex = i + 1;

            SetSlotUI(PreviewSlots[slotIndex], tiles[slotIndex], enemySkin.EnemySprite, skinData.EnemyColor);
        }
    }

    private void SetSlotUI(PreviewSlotUI slot, Sprite tileSprite, Sprite pieceSprite, Color pieceColor)
    {
        // 타일 세팅
        if (slot.TileImage != null)
        {
            slot.TileImage.sprite = tileSprite;
            // 스프라이트가 비어있으면 투명하게, 있으면 원래 색상(하얀색)으로
            slot.TileImage.color = (tileSprite != null) ? Color.white : Color.clear;
        }

        // 기물 세팅
        if (slot.PieceImage != null)
        {
            slot.PieceImage.sprite = pieceSprite;
            // 스프라이트가 비어있으면 투명하게, 있으면 지정된 틴트 컬러 적용
            slot.PieceImage.color = (pieceSprite != null) ? pieceColor : Color.clear;
            slot.PieceImage.preserveAspect = true; // 비율 유지
        }
    }

    private void ClearPreviewSlots()
    {
        if (PreviewSlots == null) return;
        foreach (var slot in PreviewSlots)
        {
            SetSlotUI(slot, null, null, Color.clear);
        }
    }

    // 유저의 현재 플레이 진척도(최고 점수 등)를 가져오는 함수
    private int GetCurrentProgress(AchievementType type)
    {
        switch (type)
        {
            case AchievementType.Score:
                // 기존 GameOverUIController에 저장된 베스트 스코어 키
                return PlayerPrefs.GetInt("BestScore", 0);
            // 콤보나 킬 카운트 등 다른 타입이 추가되면 여기에 PlayerPrefs를 불러오는 로직 추가
            default:
                return 0;
        }
    }

    public void ShowNext()
    {
        CurrentIndex++;
        if (CurrentIndex >= SkinList.Count) CurrentIndex = 0; // 끝에 도달하면 처음으로
        UpdateUI();
    }

    public void ShowPrevious()
    {
        CurrentIndex--;
        if (CurrentIndex < 0) CurrentIndex = SkinList.Count - 1; // 처음에 도달하면 끝으로
        UpdateUI();
    }

    private void EquipCurrentItem()
    {
        AchievementEntry currentData = SkinList[CurrentIndex];
        if (currentData.RewardItem is SkinData skinData)
        {
            // PlayerPrefs 직접 저장이 아닌 매니저의 이벤트 방송 함수 호출!
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.EquipSkin(skinData.ItemID);
            }
            UpdateUI();
        }
    }

    public void Hide()
    {
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }
}