using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int MaxHealth = 3;
    public int CurrentHealth;
    
    public Action<int> OnHealthChanged;
    public Action OnDied;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void TakeDamage(int DamageAmount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= DamageAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}
