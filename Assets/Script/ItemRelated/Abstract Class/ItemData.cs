using UnityEngine;

public abstract class ItemDataBase : ScriptableObject
{
    public string itemName;
    public string descricao;
    public int quantity;
    public bool Isunique;
    public GameObject item;
    public Sprite itemIcon;
}
