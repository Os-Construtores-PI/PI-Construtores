using UnityEngine;

public class InventoryItemDropZone : ItemDropZone
{
  protected override void AddItem(Player player)
  {
    player.Inventory.AddItem(itemData, itemData.quantity);
    gameObject.SetActive(false);
  }
}
