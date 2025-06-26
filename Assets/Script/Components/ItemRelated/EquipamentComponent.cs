using UnityEngine;

[RequireComponent(typeof(InventoryComponent))]
public class EquipamentComponent : ComponentBehaviour
{
    public Transform handTransform;
    private GameObject equippedWeapon;
    public EquipableItemData currentItem;
    [SerializeField] private StatComponent statComponent;

    private void Awake()
    {
        TryGetComponent(out statComponent);
    }

    public void Equip(EquipableItemData item)
    {
        Unequip();

        if (item.item != null)
        {
            equippedWeapon = Instantiate(item.item, handTransform);
            equippedWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            currentItem = item;

            foreach (var stat in item.itemStats)
            {
                statComponent.IncreaseStat(stat.stat, stat.tier, gameObject, StatComponent.StatTime.PERMANENT, stat.duration, stat.cooldown);
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
                statComponent.DecreaseStat(stat.stat, stat.tier, gameObject);
            }

            currentItem = null;
        }
    }
}
