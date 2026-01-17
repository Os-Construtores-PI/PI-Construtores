using UnityEngine;

public class AmethystItemDropZone : ItemDropZone
{
    protected override void AddItem(Player player)
    {
        player.AddAmethysts(quantity);
        gameObject.SetActive(false);
    }
}
