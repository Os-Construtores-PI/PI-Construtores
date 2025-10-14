// Classe serializável que representa um item dentro do inventário,
// armazenando os dados do item e a quantidade possuída.
[System.Serializable]
public class InventoryItem
{
    // Referência aos dados do item (nome, stats, tipo, etc)
    public ItemData data;

    // Quantidade desse item no inventário
    public int quantity;

    // Construtor para inicializar o item com dados e quantidade
    public InventoryItem(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = quantity;
    }
}

// Componente que pode ser anexado a um GameObject para representá-lo como um item no jogo
public class Item : Entities
{
    // Dados do item representado por esse componente
    public ItemData itemData;

    // Quantidade do item (geralmente 1 para objetos no mundo)
    public int quantity = 1;
}
