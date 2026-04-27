using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [HideInInspector]
    public HealthComponent PlayerHealthComponent;

    public GameObject HeartPrefab;
    public Transform HeartContainer;

    public Sprite FullHeartSprite;
    public Sprite EmptyHeartSprite;

    private List<Image> HeartIcons = new List<Image>();

    public void InitializeHealthUI(HealthComponent PlayerHealth)
    {
        PlayerHealthComponent = PlayerHealth;

        if (PlayerHealthComponent != null)
        {
            InitializeHearts();

            PlayerHealthComponent.OnHealthChanged += UpdateHearts;
            UpdateHearts(PlayerHealthComponent.CurrentHealth);
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealthComponent != null)
        {
            PlayerHealthComponent.OnHealthChanged -= UpdateHearts;
        }
    }

    private void InitializeHearts()
    {
        foreach (Transform Child in HeartContainer)
        {
            Destroy(Child.gameObject);
        }
        HeartIcons.Clear();

        for (int i = 0; i < PlayerHealthComponent.MaxHealth; i++)
        {
            GameObject NewHeart = Instantiate(HeartPrefab, HeartContainer);
            NewHeart.name = $"Heart_{i}";

            Image HeartImage = NewHeart.GetComponent<Image>();
            if (HeartImage != null)
            {
                HeartIcons.Add(HeartImage);
            }
        }
    }
    private void UpdateHearts(int CurrentHealth)
    {
        for (int i = 0; i < HeartIcons.Count; i++)
        {
            Animator Anim = HeartIcons[i].GetComponent<Animator>();   
            if (i < CurrentHealth)
            {
                if (Anim != null && Anim.isActiveAndEnabled)
                {
                    Anim.SetBool("IsEmpty", false);
                }
                else if (Anim == null)
                {
                    HeartIcons[i].sprite = FullHeartSprite;
                }
            }
            else
            {
                if (Anim != null && Anim.isActiveAndEnabled)
                {
                    Anim.SetBool("IsEmpty", true);
                }
                else if (Anim == null)
                {
                    HeartIcons[i].sprite = EmptyHeartSprite;
                }
            }
        }
    }
    
    
}
