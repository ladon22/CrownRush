using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    public int CurrentX;
    public int CurrentY;
    protected ItemManager ItemManager;

    public void Initialize(ItemManager ItemManager, int StartX, int StartY, Vector2 WorldPosition)
    {
        this.ItemManager = ItemManager;
        CurrentX = StartX;
        CurrentY = StartY;
        transform.position = WorldPosition;
    }

    public abstract void Consume(PlayerController Player);
}
