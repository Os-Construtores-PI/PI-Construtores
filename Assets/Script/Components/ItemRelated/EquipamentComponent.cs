using UnityEngine;

// Garante que o GameObject tenha um componente InventoryComponent
[RequireComponent(typeof(InventoryComponent))]
public class EquipamentComponent : ComponentBehaviour
{
    // Transform onde o equipamento será "segurado" (ex: mão do personagem)
    public Transform handTransform;

    // Referência ao objeto da arma/equipamento atualmente equipado
    private GameObject equippedWeapon;

    // Dados do item atualmente equipado
    public ItemData currentItem;

    // Referência ao componente de status para aplicar os efeitos do equipamento
    [SerializeField] private StatComponent statComponent;

    private void Awake()
    {
        // Tenta obter o componente StatComponent presente no mesmo GameObject
        TryGetComponent(out StatComponent statComponent);
    }

    // Método para equipar um item
    public void Equip(ItemData item)
    {
        // Primeiro remove qualquer equipamento que já esteja equipado
        Unequip();

        // Verifica se o item possui um prefab associado e se é equipável
        if (item.item != null || item.usageType != ItemUsageType.Equipable)
        {
            // Instancia o objeto do equipamento na mão (handTransform)
            equippedWeapon = Instantiate(item.item, handTransform);

            // Ajusta posição e rotação local para zero (alinha com a mão)
            equippedWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Guarda referência do item equipado
            currentItem = item;

            // Aplica os efeitos permanentes de status do equipamento ao personagem
            foreach (var stat in item.itemStats)
            {
                statComponent.ApplyStat(stat.stat, stat.tier, statComponent.gameObject, StatComponent.StatTime.PERMANENT, stat.duration, stat.cooldown);
            }
        }
    }

    // Método para desequipar o item atual
    public void Unequip()
    {
        // Se houver arma/equipamento equipado, destrói o objeto na cena
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
            equippedWeapon = null;
        }

        // Remove os efeitos do item equipado nos status do personagem
        if (currentItem != null)
        {
            foreach (var stat in currentItem.itemStats)
            {
                statComponent.RemoveStat(stat.stat, stat.tier, statComponent.gameObject);
            }

            // Limpa a referência do item atual
            currentItem = null;
        }
    }
}
