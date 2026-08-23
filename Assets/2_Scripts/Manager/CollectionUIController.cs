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
    // ���̺��� �뵵���� ������ �̸��� �޽��ϴ�.
    public string UITableName = "UI_Text";
    public string ItemTableName = "Item_Table";
    public string AchievementTableName = "Achievement_Table";

    [Header("Item Preview UI")]
    // ���� TextMeshProUGUI ��� LocalizeStringEvent�� ����մϴ�.
    public LocalizeStringEvent ItemNameTextEvent;

    [Header("Unlock Status UI (���/���൵ �ڽ�)")]
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

    // �����ͺ��̽����� ��Ų�� ���� ��Ƶ� ����Ʈ
    private List<AchievementEntry> SkinList = new List<AchievementEntry>();
    private int CurrentIndex = 0;
    private Sprite defaultPreviewFrameSprite;

    private void Awake()
    {
        // ������ �� ���� UI�� ����Ǿ� �ִ� �⺻ �̹����� �����صӴϴ�.
        if (PreviewFrameImage != null)
        {
            defaultPreviewFrameSprite = PreviewFrameImage.sprite;
        }
    }
    private void Start()
    {
        // ��ư �̺�Ʈ ����
        if (LeftButton != null) LeftButton.onClick.AddListener(ShowPrevious);
        if (RightButton != null) RightButton.onClick.AddListener(ShowNext);
        if (SelectButton != null) SelectButton.onClick.AddListener(EquipCurrentItem);
    }

    public void Show()
    {
        if (gameObject.activeInHierarchy) return;

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);

        // 1. �����ͺ��̽����� ��Ų ��ϸ� �����ͼ� ����Ʈ ����
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
        CurrentIndex = 0; // Ȥ�� �� ã�� ��츦 ����� �⺻��

        //3. [�߰�] ����Ʈ�� �Ⱦ�� ���� ���� ��Ų�� �� ��°�� �ִ��� ã���ϴ�.
        for (int i = 0; i < SkinList.Count; i++)
        {
            if (SkinList[i].RewardItem != null && SkinList[i].RewardItem.ItemID == equippedID)
            {
                CurrentIndex = i;
                break;
            }
        }

        UpdateUI();

        // �˾� �ִϸ��̼�
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // ȭ��(����, ������, ��ư ����)�� �����ϴ� �ٽ� �Լ�
    private void UpdateUI()
    {
        if (SkinList.Count == 0) return;

        AchievementEntry currentData = SkinList[CurrentIndex];

        bool isDefaultSkin = (currentData.RewardItem.ItemID == "skin_default");



        // �̸� ����
        if (ItemNameTextEvent != null && currentData.RewardItem != null)
        {
            ItemNameTextEvent.StringReference.SetReference(ItemTableName, currentData.RewardItem.ItemID);
        }

        if (currentData.RewardItem is SkinData skinData) // (���� ���� ����)
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
                PreviewFrameImage.type = Image.Type.Tiled;
                PreviewFrameImage.pixelsPerUnitMultiplier = 0.29f;
                PreviewFrameImage.color = skinData.FrameColor;
            }
        }
        else
        {
            // ��Ų�� �ƴ� �Ϲ� �������� ����� ó�� (�ʿ��ϴٸ�)
            ClearPreviewSlots();
        }

        int currentProgress = GetCurrentProgress(currentData.Type);
        // �ر� ���� �� ���� ���� Ȯ��
        string unlockKey = $"{currentData.RewardItem.ItemID}_Unlocked";
        // RewardID�� "Skin_Default" (�Ǵ� "Board_Default")�� �⺻ �������� ������ true(�ر�)�� ó���մϴ�!
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
            // �⺻ ��Ų�� �ƴ� ���� �ؽ�Ʈ�� �������� �����մϴ�.
            if (UnlockConditionTextEvent != null)
                UnlockConditionTextEvent.StringReference.SetReference(AchievementTableName, currentData.TitleKey);

            if (ProgressText != null)
                ProgressText.text = $"{(currentProgress < currentData.TargetValue ? currentProgress : currentData.TargetValue)} / {currentData.TargetValue}";

            if (ProgressSlider != null)
                ProgressSlider.value = Mathf.Clamp01((float)currentProgress / currentData.TargetValue);

            // �ر� ���ο� ������� �������� �г�(���� �ؽ�Ʈ �� ����)�� �ϴ� �ѵӴϴ�! 
            // (�׷��� "���� �޼�", "���� �ڹ���" ���� ���̴ϱ��)
            if (LockOverlayPanel != null) LockOverlayPanel.SetActive(true);
        }

        if (isUnlocked)
        {
            SelectButton.interactable = !isEquipped; // �̹� ���� ���̸� ��ư ��Ȱ��ȭ
            // [����] ��ư�� �ؽ�Ʈ�� ���� ���¿� �°� �ٲ��ݴϴ�!
            if (SelectTextEvent != null)
            {
                SelectTextEvent.StringReference.SetReference(UITableName, isEquipped ? "ui_btn_equipped" : "ui_btn_select");
            }
        }
        else
        {
            // ��� ���� (�̹����� �Ȱ��� ���൵ �� ǥ��)
            if (LockOverlayPanel != null) LockOverlayPanel.SetActive(true);
            SelectButton.interactable = false;

            if (SelectTextEvent != null)
                SelectTextEvent.StringReference.SetReference(UITableName, "ui_btn_locked");
        }
    }

    private void UpdateSkinDiorama(SkinData skinData)
    {
        // �ν����� ������ �����Ǿ����� ���� �˻�
        if (PreviewSlots == null || PreviewSlots.Length < 6)
        {
            return;
        }

        //  3x2 ü���� ���Ͽ� �°� �����Ǵ� Ÿ�� ���� �迭 (Light, Dark ����)
        Sprite[] tiles = {
            skinData.LightTileSprite, skinData.DarkTileSprite, skinData.LightTileSprite,
            skinData.DarkTileSprite, skinData.LightTileSprite, skinData.DarkTileSprite
        };

        // 1. ù ��° ���� [0] : �÷��̾� (King)
        SetSlotUI(PreviewSlots[0], tiles[0], skinData.PlayerSprite, skinData.PlayerColor);

        // 2. ������ ���� [1]~[5] : �� �⹰��
        // Enum ������ UI�� �����ְ� ���� ������� ��ġ�մϴ�.
        EnemyPieceType[] enemyTypes = {
            EnemyPieceType.Pawn,     // [1] 폰
            EnemyPieceType.Knight,   // [2] 나이트
            EnemyPieceType.Rook,     // [3] 룩
            EnemyPieceType.Bishop,   // [4] 비숍
            EnemyPieceType.Queen     // [5] 퀸
        };

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            // ��Ų �����Ϳ��� �ش� Ÿ���� �� ��������Ʈ�� ã�ƿɴϴ�.
            EnemySkinData enemySkin = skinData.GetEnemySkin(enemyTypes[i]);

            // �÷��̾ 0���̹Ƿ� ���� 1�� �ε������� ����
            int slotIndex = i + 1;

            SetSlotUI(PreviewSlots[slotIndex], tiles[slotIndex], enemySkin.EnemySprite, skinData.EnemyColor);
        }
    }

    private void SetSlotUI(PreviewSlotUI slot, Sprite tileSprite, Sprite pieceSprite, Color pieceColor)
    {
        // Ÿ�� ����
        if (slot.TileImage != null)
        {
            slot.TileImage.sprite = tileSprite;
            // ��������Ʈ�� ��������� �����ϰ�, ������ ���� ����(�Ͼ��)����
            slot.TileImage.color = (tileSprite != null) ? Color.white : Color.clear;
        }

        // �⹰ ����
        if (slot.PieceImage != null)
        {
            slot.PieceImage.sprite = pieceSprite;
            // ��������Ʈ�� ��������� �����ϰ�, ������ ������ ƾƮ �÷� ����
            slot.PieceImage.color = (pieceSprite != null) ? pieceColor : Color.clear;
            slot.PieceImage.preserveAspect = true; // ���� ����
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

    // ������ ���� �÷��� ��ô��(�ְ� ���� ��)�� �������� �Լ�
    private int GetCurrentProgress(AchievementType type)
    {
        switch (type)
        {
            case AchievementType.Score:
                // ���� GameOverUIController�� ����� ����Ʈ ���ھ� Ű
                return PlayerPrefs.GetInt("BestScore", 0);
            // �޺��� ų ī��Ʈ �� �ٸ� Ÿ���� �߰��Ǹ� ���⿡ PlayerPrefs�� �ҷ����� ���� �߰�
            default:
                return 0;
        }
    }

    public void ShowNext()
    {
        CurrentIndex++;
        if (CurrentIndex >= SkinList.Count) CurrentIndex = 0; // ���� �����ϸ� ó������
        UpdateUI();
    }

    public void ShowPrevious()
    {
        CurrentIndex--;
        if (CurrentIndex < 0) CurrentIndex = SkinList.Count - 1; // ó���� �����ϸ� ������
        UpdateUI();
    }

    private void EquipCurrentItem()
    {
        AchievementEntry currentData = SkinList[CurrentIndex];
        if (currentData.RewardItem is SkinData skinData)
        {
            // PlayerPrefs ���� ������ �ƴ� �Ŵ����� �̺�Ʈ ��� �Լ� ȣ��!
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
            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                if (GameManager.Instance != null)
                    GameManager.Instance.InputController.PopInputLock();
            });
    }
}