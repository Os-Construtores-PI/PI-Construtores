using System.Collections.Generic;
using UnityEngine;

// Cria um asset no menu "Inventory/Item Data" para facilitar a criação de itens no Unity Editor
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // Tipo de uso do item (equipável, consumível, etc)
    public ItemUsageType usageType;

    // Nome do item
    public string itemName;

    // Lista de estatísticas ou efeitos que o item pode aplicar
    public List<StatEntry> itemStats = new();

    // Quantidade padrão do item (usado na criação)
    public int quantity;

    // Indica se o item é único (não empilhável)
    public bool Isunique;

    // Prefab do objeto 3D ou visual do item (para equipar, por exemplo)
    public GameObject item;

    // Ícone do item para UI/inventário
    public Sprite itemIcon;

    // Classe serializável para definir um efeito ou status aplicado pelo item
    [System.Serializable]
    public class StatEntry
    {
        // Tipo da estatística que o item modifica (ex: ataque, defesa)
        public StatType stat;

        // Duração do efeito (se temporário)
        public float duration;

        // Tempo de cooldown antes de poder reaplicar o efeito
        public float cooldown;

        // Raridade ou qualidade do efeito que influencia o multiplicador
        public QualityTier tier;
    }
}
