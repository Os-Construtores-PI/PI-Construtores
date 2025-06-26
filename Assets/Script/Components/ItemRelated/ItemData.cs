using UnityEngine;

public abstract class ItemDataBase : ScriptableObject
{
    public string itemName;
    public int quantity;
    public bool Isunique;
    public GameObject item;
    public Sprite itemIcon;
}
