using UnityEngine;


[RequireComponent(typeof(InventoryComponent))]
public class EquipamentComponent : ComponentBehaviour
{
    public Transform handTransform;
    private GameObject equippedWeapon;
    public ItemData currentItem;

    [SerializeField] private StatComponent statComponent;
    private void Awake()
    {
        TryGetComponent(out StatComponent statComponent);
    }

    public void Equip(ItemData item)
    {
        Unequip();
        if (item.item != null || item.usageType != ItemUsageType.Equipable)
        {
            equippedWeapon = Instantiate(item.item, handTransform);
            equippedWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            currentItem = item;
            foreach (var stat in item.itemStats)
            {
                statComponent.ApplyStat(stat.stat, stat.tier, statComponent.gameObject, StatComponent.StatTime.PERMANENT);
            }
        }
    }

    public void Unequip()
    {
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
            equippedWeapon = null;
        }

        if (currentItem != null)
        {
            foreach (var stat in currentItem.itemStats)
            {
                statComponent.RemoveStat(stat.stat, stat.tier, statComponent.gameObject);
            }
            currentItem = null;
        }
    }
}
