using UnityEngine;

// Garante que o componente InventoryComponent esteja presente no GameObject
[RequireComponent(typeof(InventoryComponent))]
public class EquipamentComponent : ComponentBehaviour
{
    // Transform onde o item será instanciado (ex: a mão do personagem)
    public Transform handTransform;

    // Referência para o GameObject da arma equipada
    private GameObject equippedWeapon;

    // Dados do item atualmente equipado
    public EquipableItemData currentItem;

    // Referência para o componente de estatísticas (stats)
    [SerializeField] private StatComponent statComponent;

    // Tenta obter o componente StatComponent ao inicializar
    private void Awake()
    {
        TryGetComponent(out statComponent);
    }

    // Método para equipar um item
    public void Equip(EquipableItemData item)
    {
        // Remove o item atual, se houver
        Unequip();

        // Verifica se o prefab do item é válido
        if (item.item != null)
        {
            // Instancia o prefab do item na mão do personagem
            equippedWeapon = Instantiate(item.item, handTransform);
            equippedWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Salva o item atual como equipado
            currentItem = item;

            // Aplica os modificadores de status definidos no item
            foreach (var stat in item.itemStats)
            {
                statComponent.IncreaseStat(
                    stat.stat,                    // Tipo de stat (ex: força, agilidade)
                    stat.tier,                    // Qualidade do stat (ex: comum, raro)
                    gameObject,                   // Fonte do modificador
                    StatComponent.StatTime.Temporary // Duração do buff (aqui é permanente)
                );
            }
        }
    }

    // Método para desequipar o item atual
    public void Unequip()
    {
        // Se houver uma arma equipada, destrói o GameObject
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
            equippedWeapon = null;
        }

        // Se houver item equipado, remove seus efeitos de stat
        if (currentItem != null)
        {
            foreach (var stat in currentItem.itemStats)
            {
                statComponent.DecreaseStat(
                    stat.stat,    // Tipo de stat
                    stat.tier,    // Qualidade do stat
                    gameObject    // Fonte do modificador a ser removido
                );
            }

            // Limpa a referência ao item atual
            currentItem = null;
        }
    }
}
