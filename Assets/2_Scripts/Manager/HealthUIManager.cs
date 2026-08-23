using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [HideInInspector]
    public HealthComponent PlayerHealthComponent;

    public GameObject HeartPrefab;
    public Transform HeartContainer;

    public Sprite FullHealthSprite;
    public Sprite EmptyHealthSprite;

    private List<Image> HealthIcons = new List<Image>();

    public void InitializeHealthUI(HealthComponent PlayerHealth)
    {
        PlayerHealthComponent = PlayerHealth;

        if (PlayerHealthComponent != null)
        {
            InitializeHearts();

            PlayerHealthComponent.OnHealthChanged += UpdateIcons;
            UpdateIcons(PlayerHealthComponent.CurrentHealth);
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealthComponent != null)
        {
            PlayerHealthComponent.OnHealthChanged -= UpdateIcons;
        }
    }

    private void InitializeHearts()
    {
        foreach (Transform Child in HeartContainer)
        {
            Destroy(Child.gameObject);
        }
        HealthIcons.Clear();

        for (int i = 0; i < PlayerHealthComponent.MaxHealth; i++)
        {
            GameObject NewHeart = Instantiate(HeartPrefab, HeartContainer);
            NewHeart.name = $"Heart_{i}";

            Image HeartImage = NewHeart.GetComponent<Image>();
            if (HeartImage != null)
            {
                HealthIcons.Add(HeartImage);
            }
        }
    }

    private void UpdateIcons(int CurrentHealth)
    {
        for (int i = 0; i < HealthIcons.Count; i++)
        {
            // 현재 체력보다 인덱스가 작으면 꽉 찬 하트, 아니면 빈 하트로 표시합니다.
            if (i < CurrentHealth)
            {
                HealthIcons[i].sprite = FullHealthSprite;
            }
            else
            {
                HealthIcons[i].sprite = EmptyHealthSprite;
            }
        }
    }
}
