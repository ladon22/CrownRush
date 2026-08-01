using UnityEngine;

public class HealthItem : ItemBase
{
    [SerializeField] private int HealAmount = 1;
    public override void Consume(PlayerController Player)
    {
        if (Player.HealthComponent != null)
        {
            Player.HealthComponent.Heal(HealAmount);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SfxType.ItemConsume);
        }

        if(ItemManager != null) ItemManager.RemoveItem(this);
        KillTweens();
        PoolManager.Instance.ReturnToPool(gameObject);
    }
}
