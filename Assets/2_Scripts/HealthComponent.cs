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

        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeForHit();
        }

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.Vibrate(VibrateType.Heavy);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SfxType.PlayerHit, true);
        }

        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(int HealAmount)
    {
        if (CurrentHealth <= 0 || CurrentHealth >= MaxHealth) return;

        CurrentHealth += HealAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void Revive(int amount)
    {
        CurrentHealth = Mathf.Clamp(amount, 1, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}
