using System.Collections.Generic;
using UnityEngine;

// Garante que o componente BrainComponent esteja presente no GameObject
[RequireComponent(typeof(BrainComponent))]
public class InventoryComponent : ComponentBehaviour
{
    // Lista de itens no inventário
    [SerializeField]
    private List<InventoryItem> items = new();

    // Tipo da entidade (ex: PLAYER, ENEMY, etc.)
    private EntityType type;

    // Ao iniciar, armazena o tipo de entidade com base no BrainComponent
    private void Awake()
    {
        if (TryGetComponent(out BrainComponent brain))
            type = brain.identity.TipoEntidade;
    }

    // Adiciona um item ao inventário
    public void AddItem(ItemDataBase data, int quantity = 1)
    {
        // Se o item não é único, tenta acumular com outro igual
        if (!data.Isunique)
        {
            var existing = items.Find(i => i.data == data);
            if (existing != null)
            {
                existing.quantity += quantity;
                return;
            }
        }

        // Caso contrário, adiciona um novo item à lista
        items.Add(new InventoryItem(data, quantity));
        Debug.Log($"Adicionado: {data.itemName} x{quantity}");
    }

    // Remove uma quantidade de um item do inventário
    public void RemoveItem(ItemDataBase data, int quantity = 1)
    {
        var item = items.Find(i => i.data == data);
        if (item != null)
        {
            item.quantity -= quantity;

            // Se a quantidade chegar a zero ou menos, remove o item completamente
            if (item.quantity <= 0)
                items.Remove(item);
        }
    }

    // Usa um item do inventário, com lógica diferente para cada tipo
    public void UseItem(ItemDataBase data)
    {
        // Verifica se o item existe no inventário
        if (!items.Exists(i => i.data == data)) return;

        switch (data)
        {
            // Se for equipável e a entidade for jogador, equipa o item
            case EquipableItemData equipable when type == EntityType.PLAYER:
                if (TryGetComponent<EquipamentComponent>(out var equipComp))
                    equipComp.Equip(equipable);
                break;

            // Se for consumível, aplica os efeitos de stat e remove 1 unidade
            case ConsumableItemData consumable:
                if (TryGetComponent<StatComponent>(out var statComp))
                {
                    foreach (var stat in consumable.itemStats)
                    {
                        statComp.IncreaseStat(
                            stat.stat,
                            stat.tier,
                            gameObject,
                            StatComponent.StatTime.TEMPORARY,
                            stat.duration,
                            stat.cooldown
                        );
                    }
                    RemoveItem(data, 1); // Consome o item
                }
                break;

            // Itens passivos não são usáveis diretamente
            case PassiveItemData:
                Debug.Log("Item passivo não é utilizável diretamente.");
                break;

            // Caso o tipo de item não seja reconhecido
            default:
                Debug.LogWarning("Tipo de item desconhecido.");
                break;
        }
    }

    // Retorna a lista de itens atuais do inventário
    public List<InventoryItem> GetItems() => items;

    // Limpa todos os itens do inventário
    public void ClearItems() => items.Clear();
}
