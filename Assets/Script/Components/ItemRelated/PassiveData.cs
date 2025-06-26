using UnityEngine;

[CreateAssetMenu(fileName = "NewPassiveItem", menuName = "Inventory/Item/Passive")]
public class PassiveItemData : ItemDataBase
{
    public string description;
    // você pode adicionar buffs passivos permanentes aqui se quiser
}
