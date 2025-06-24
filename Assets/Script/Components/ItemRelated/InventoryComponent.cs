using System.Collections.Generic;
using UnityEngine;

// Garante que o GameObject tenha um BrainComponent
[RequireComponent(typeof(BrainComponent))]
public class InventoryComponent : ComponentBehaviour
{
    // Lista que armazena os itens do inventário
    [SerializeField]
    private List<InventoryItem> items = new();

    // Guarda o tipo da entidade (ex: jogador, inimigo) baseado no BrainComponent
    private EntityType type;

    // Executado ao iniciar o componente
    private void Awake()
    {
        // Pega o BrainComponent para determinar o tipo da entidade
        if (TryGetComponent(out BrainComponent brain))
        {
            type = brain.identity.TipoEntidade;
        }
    }

    // Adiciona um item ao inventário
    public void AddItem(ItemData data, int quantity = 1)
    {
        // Se o item não for único (pode ter mais de uma unidade)
        if (!data.Isunique)
        {
            // Verifica se já existe esse item na lista
            var existing = items.Find(i => i.data == data);
            if (existing != null)
            {
                // Se já existir, só aumenta a quantidade
                existing.quantity += quantity;
                return;
            }
        }
        // Se for único ou não estiver na lista, adiciona novo item
        items.Add(new InventoryItem(data, quantity));

        // Imprime debug informando o que foi adicionado
        print($"Adicionado: {data.itemName} x {quantity}, {data.itemStats}, {data.usageType}, {data.Isunique}");
    }

    // Remove uma quantidade do item do inventário
    public void RemoveItem(ItemData data, int quantity = 1)
    {
        // Busca o item na lista
        var item = items.Find(i => i.data == data);
        if (item != null)
        {
            // Reduz a quantidade
            item.quantity -= quantity;

            // Remove o item da lista se a quantidade for zero ou menos
            if (item.quantity <= 0)
                items.Remove(item);
        }
    }

    // Usa um item do inventário (equipar ou consumir)
    public void UseItem(ItemData data)
    {
        // Se o item não existir no inventário, retorna
        if (!items.Exists(i => i.data == data)) return;

        // Se o item for equipável e a entidade for jogador
        if (data.usageType == ItemUsageType.Equipable && type == EntityType.PLAYER)
        {
            // Tenta pegar o componente de equipamento e equipar o item
            if (TryGetComponent<EquipamentComponent>(out var equipment))
            {
                equipment.Equip(data);
            }
        }
        // Se o item for consumível
        else if (data.usageType == ItemUsageType.Consumable)
        {
            // Tenta aplicar os efeitos de status através do StatComponent
            if (TryGetComponent(out StatComponent statComponent))
            {
                // Aplica todos os efeitos de status do item (temporários)
                foreach (var stat in data.itemStats)
                {
                    statComponent.ApplyStat(stat.stat, stat.tier, statComponent.gameObject, StatComponent.StatTime.TEMPORARY);
                }
                // Remove uma unidade do item consumido
                RemoveItem(data, 1);
            }
        }
        else
        {
            // Caso o item não possa ser usado dessa forma
            Debug.LogWarning("Item não é utilizável.");
        }
    }

    // Retorna a lista atual de itens do inventário
    public List<InventoryItem> GetItems()
    {
        return items;
    }

    // Limpa todo o inventário
    public void ClearItems()
    {
        items.Clear();
    }
}
