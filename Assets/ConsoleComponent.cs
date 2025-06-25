using UnityEngine;
using IngameDebugConsole;

// Componente que registra comandos acessíveis via Ingame Debug Console
public class ConsoleComponent : MonoBehaviour
{
    public GameObject target; // GameObject alvo dos comandos (ex: jogador selecionado)

    // Propriedade estática que retorna o sistema de dados ativo (para save/load e lookup de jogadores)
    private static DataSystem Data => FindAnyObjectByType<DataSystem>();

    // Retorna o GameObject alvo atual do console
    private static GameObject Target => FindAnyObjectByType<ConsoleComponent>().target;

    // Comando para aplicar um modificador de status ao alvo
    [ConsoleMethod("increaseStat", "Aplica um status ao alvo", "STATTYPE TIER TIME [duration] [cooldown]")]
    public static void IncreaseStat(string statType, string tier, string timeType, string duration = "0", string cooldown = "0")
    {
        // Verifica se há um alvo definido
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        // Verifica se o alvo possui o componente de status
        if (!Target.TryGetComponent(out StatComponent statComp))
        {
            Debug.LogWarning("Alvo não possui StatComponent.");
            return;
        }

        // Tenta converter os parâmetros string em enums válidos
        if (!System.Enum.TryParse(statType.ToUpper(), out StatType stat) ||
            !System.Enum.TryParse(tier.ToUpper(), out QualityTier qualityTier) ||
            !System.Enum.TryParse(timeType.ToUpper(), out StatComponent.StatTime time))
        {
            Debug.LogError("Parâmetros inválidos. Uso: increaseStat ATTACK COMMON TEMPORARY 5 2");
            return;
        }

        // Converte duração e cooldown (se fornecidos)
        float dur = float.TryParse(duration, out float d) ? d : 0f;
        float cd = float.TryParse(cooldown, out float c) ? c : 0f;

        // Aplica o modificador de status
        statComp.IncreaseStat(stat, qualityTier, Target, time, dur, cd);
        Debug.Log($"IncreaseStat: {stat} ({qualityTier}) como {time} aplicado.");
    }

    // Comando para remover um modificador de status do alvo
    [ConsoleMethod("decreaseStat", "Remove ou reduz um status do alvo", "STATTYPE")]
    public static void DecreaseStat(string statType)
    {
        if (Target == null || !Target.TryGetComponent(out StatComponent statComp))
        {
            Debug.LogWarning("Alvo ou StatComponent ausente.");
            return;
        }

        // Caso especial: "ALL" remove todos os tipos de status
        if (statType.ToUpper() == "ALL")
        {
            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
                statComp.DecreaseStat(stat, QualityTier.COMMON, Target); // Usa Tier genérico como base

            Debug.Log("Todos os stats removidos.");
            return;
        }

        // Tenta converter o tipo de status
        if (!System.Enum.TryParse(statType.ToUpper(), out StatType statEnum))
        {
            Debug.LogError("StatType inválido.");
            return;
        }

        // Remove o status específico
        statComp.DecreaseStat(statEnum, QualityTier.COMMON, Target);
        Debug.Log($"DecreaseStat: {statEnum} removido.");
    }

    // Comando para alterar atributos de um componente específico do alvo
    [ConsoleMethod("setAttribute", "Seta um atributo de um componente do alvo", "componente atributo valor")]
    public static void SetAttribute(string componentName, string attributeName, string value)
    {
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        // Tenta obter o componente solicitado
        ComponentBehaviour comp = Target.GetComponent(componentName) as ComponentBehaviour;
        if (comp == null)
        {
            Debug.LogWarning($"Componente {componentName} não encontrado no alvo.");
            return;
        }

        // Tenta detectar o tipo atual do atributo e definir o novo valor dinamicamente
        var attr = comp.GetAttribute<object>(attributeName);

        if (attr is int)
            comp.SetAttribute(attributeName, int.Parse(value));
        else if (attr is float)
            comp.SetAttribute(attributeName, float.Parse(value));
        else if (attr is bool)
            comp.SetAttribute(attributeName, bool.Parse(value));
        else
            comp.SetAttribute(attributeName, value); // Se não for tipo conhecido, aplica como string

        Debug.Log($"Atributo '{attributeName}' de '{componentName}' setado para {value}");
    }

    // Comando para teletransportar o alvo para uma posição no mundo
    [ConsoleMethod("teleportTo", "Teleporta o alvo para X Y Z", "x y z")]
    public static void TeleportTo(float x, float y, float z)
    {
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        Vector3 pos = new(x, y, z);
        if (Target.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            Target.transform.position = pos;
            controller.enabled = true;
        }
        else
        {
            Target.transform.position = pos;
        }
        Debug.Log($"Teleportado para ({x}, {y}, {z})");
    }

    // Comando para salvar o estado de todos os jogadores usando o DataSystem
    [ConsoleMethod("saveGame", "Salva o estado do jogo para todos os jogadores", "")]
    public static void SaveGame()
    {
        var dataSystem = FindAnyObjectByType<DataSystem>();
        if (dataSystem == null)
        {
            Debug.LogError("DataSystem não encontrado na cena.");
            return;
        }

        dataSystem.Save();
    }

    // Comando para carregar o estado salvo de todos os jogadores
    [ConsoleMethod("loadGame", "Carrega o estado do jogo para todos os jogadores", "")]
    public static void LoadGame()
    {
        var dataSystem = FindAnyObjectByType<DataSystem>();
        if (dataSystem == null)
        {
            Debug.LogError("DataSystem não encontrado na cena.");
            return;
        }

        dataSystem.Load();
    }

    // Define o alvo atual pelo ID do jogador, utilizando o DataSystem
    [ConsoleMethod("setTarget", "Define o jogador alvo pelos dados do DataSystem", "playerId")]
    public static void SetTarget(string playerId)
    {
        var player = Data.players.Find(p => p.playerId == playerId);
        if (player == null)
        {
            Debug.LogWarning($"Jogador com ID '{playerId}' não encontrado.");
            return;
        }

        FindAnyObjectByType<ConsoleComponent>().target = player.transform.gameObject;
        Debug.Log($"Target definido para: {playerId}");
    }

    // Dá um item do Resources/Items ao inventário do jogador pelo ID
    [ConsoleMethod("giveItem", "Adiciona um item ao inventário do jogador", "playerId itemName quantidade")]
    public static void GiveItem(string playerId, string itemName, string quantidade)
    {
        var player = Data.players.Find(p => p.playerId == playerId);
        if (player == null)
        {
            Debug.LogWarning($"Jogador '{playerId}' não encontrado.");
            return;
        }

        var item = Resources.Load<ItemData>("Items/" + itemName);
        if (item == null)
        {
            Debug.LogWarning($"Item '{itemName}' não encontrado na pasta Resources/Items/");
            return;
        }

        if (!int.TryParse(quantidade, out int qtd) || qtd <= 0)
        {
            Debug.LogWarning("Quantidade inválida.");
            return;
        }

        player.inventory.AddItem(item, qtd);
        Debug.Log($"Item '{itemName}' x{qtd} adicionado ao jogador '{playerId}'.");
    }

    // Lista todos os jogadores registrados no DataSystem
    [ConsoleMethod("listPlayers", "Lista os IDs dos jogadores salvos no DataSystem", "")]
    public static void ListPlayers()
    {
        if (Data.players.Count == 0)
        {
            Debug.Log("Nenhum jogador registrado no DataSystem.");
            return;
        }

        Debug.Log("Jogadores registrados:");
        foreach (var p in Data.players)
        {
            Debug.Log($"- {p.playerId}");
        }
    }
}
