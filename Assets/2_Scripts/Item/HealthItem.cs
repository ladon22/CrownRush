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
        
        if(ItemManager != null) ItemManager.RemoveItem(this);
        Destroy(gameObject);
    }
}
