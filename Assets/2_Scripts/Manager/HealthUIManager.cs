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
            // ÇöÀç Ã¼·Âº¸´Ù ÀÎµ¦½º°¡ ³·À¸¸é ²Ë Âù ¿Õ°ü, ¾Æ´Ï¸é ºó ¿Õ°üÀ» º¸¿©ÁÝ´Ï´Ù.
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

    //private void UpdateHearts(int CurrentHealth)
    //{
    //    for (int i = 0; i < HeartIcons.Count; i++)
    //    {
    //        Animator Anim = HeartIcons[i].GetComponent<Animator>();   
    //        if (i < CurrentHealth)
    //        {
    //            if (Anim != null && Anim.isActiveAndEnabled)
    //            {
    //                Anim.SetBool("IsEmpty", false);
    //            }
    //            else if (Anim == null)
    //            {
    //                HeartIcons[i].sprite = FullHeartSprite;
    //            }
    //        }
    //        else
    //        {
    //            if (Anim != null && Anim.isActiveAndEnabled)
    //            {
    //                Anim.SetBool("IsEmpty", true);
    //            }
    //            else if (Anim == null)
    //            {
    //                HeartIcons[i].sprite = EmptyHeartSprite;
    //            }
    //        }
    //    }
    //}


}
