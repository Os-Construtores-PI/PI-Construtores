using UnityEngine;
using IngameDebugConsole;

// Componente que registra comandos para o Ingame Debug Console
public class ConsoleComponent : MonoBehaviour
{
    // Referência pública para o alvo dos comandos (ex: o jogador selecionado)
    public GameObject target;

    // Propriedade estática que retorna o sistema de dados ativo na cena (gerencia save/load e jogadores)
    private static DataSystem Data => FindAnyObjectByType<DataSystem>();

    // Propriedade estática que retorna o GameObject alvo atual do console
    private static GameObject Target => FindAnyObjectByType<ConsoleComponent>().target;

    // Comando para aplicar um modificador de status no alvo via console
    // Parâmetros: tipo de status, qualidade, tipo de duração, duração e cooldown opcionais
    [ConsoleMethod("increaseStat", "Aplica um status ao alvo", "STATTYPE TIER TIME [duration] [cooldown]")]
    public static void IncreaseStat(string statType, string tier, string timeType, string duration = "0", string cooldown = "0")
    {
        // Checa se o alvo está definido
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        // Verifica se o alvo tem o componente StatComponent para aplicar modificadores
        if (!Target.TryGetComponent(out StatComponent statComp))
        {
            Debug.LogWarning("Alvo não possui StatComponent.");
            return;
        }

        // Converte strings recebidas para enums correspondentes (StatType, QualityTier e StatTime)
        if (!System.Enum.TryParse(statType.ToUpper(), out StatType stat) ||
            !System.Enum.TryParse(tier.ToUpper(), out QualityTier qualityTier) ||
            !System.Enum.TryParse(timeType.ToUpper(), out StatComponent.StatTime time))
        {
            Debug.LogError("Parâmetros inválidos. Uso: increaseStat ATTACK COMMON TEMPORARY 5 2");
            return;
        }

        // Converte strings duration e cooldown para float; usa 0 caso falhe
        float dur = float.TryParse(duration, out float d) ? d : 0f;
        float cd = float.TryParse(cooldown, out float c) ? c : 0f;

        // Aplica o modificador de status ao alvo, usando o método IncreaseStat do componente
        statComp.IncreaseStat(stat, qualityTier, Target, time, dur, cd);
        Debug.Log($"IncreaseStat: {stat} ({qualityTier}) como {time} aplicado.");
    }

    // Comando para remover um modificador de status do alvo
    [ConsoleMethod("decreaseStat", "Remove ou reduz um status do alvo", "STATTYPE")]
    public static void DecreaseStat(string statType)
    {
        // Verifica se alvo e componente estão disponíveis
        if (Target == null || !Target.TryGetComponent(out StatComponent statComp))
        {
            Debug.LogWarning("Alvo ou StatComponent ausente.");
            return;
        }

        // Se o parâmetro for "ALL", remove todos os tipos de status (loop por todos os StatType)
        if (statType.ToUpper() == "ALL")
        {
            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
                statComp.DecreaseStat(stat, QualityTier.COMMON, Target); // Usa Tier comum para remover

            Debug.Log("Todos os stats removidos.");
            return;
        }

        // Tenta converter o parâmetro para enum StatType válido
        if (!System.Enum.TryParse(statType.ToUpper(), out StatType statEnum))
        {
            Debug.LogError("StatType inválido.");
            return;
        }

        // Remove o status específico do alvo
        statComp.DecreaseStat(statEnum, QualityTier.COMMON, Target);
        Debug.Log($"DecreaseStat: {statEnum} removido.");
    }

    // Comando para alterar o valor de um atributo específico de um componente do alvo
    [ConsoleMethod("setAttribute", "Seta um atributo de um componente do alvo", "componente atributo valor")]
    public static void SetAttribute(string componentName, string attributeName, string value)
    {
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        // Tenta pegar o componente (deve herdar ComponentBehaviour)
        ComponentBehaviour comp = Target.GetComponent(componentName) as ComponentBehaviour;
        if (comp == null)
        {
            Debug.LogWarning($"Componente {componentName} não encontrado no alvo.");
            return;
        }

        // Pega o valor atual do atributo para identificar o tipo (int, float, bool ou outro)
        var attr = comp.GetAttribute<object>(attributeName);

        // Define o novo valor convertendo para o tipo correto
        if (attr is int)
            comp.SetAttribute(attributeName, int.Parse(value));
        else if (attr is float)
            comp.SetAttribute(attributeName, float.Parse(value));
        else if (attr is bool)
            comp.SetAttribute(attributeName, bool.Parse(value));
        else
            comp.SetAttribute(attributeName, value); // Se não reconhecido, define como string

        Debug.Log($"Atributo '{attributeName}' de '{componentName}' setado para {value}");
    }

    // Comando para teletransportar o alvo para uma posição específica no mundo
    [ConsoleMethod("teleportTo", "Teleporta o alvo para X Y Z", "x y z")]
    public static void TeleportTo(float x, float y, float z)
    {
        if (Target == null)
        {
            Debug.LogWarning("Alvo não definido.");
            return;
        }

        Vector3 pos = new(x, y, z);

        // Se o alvo tem CharacterController, desabilita temporariamente para evitar conflito ao mudar posição
        if (Target.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            Target.transform.position = pos;
            controller.enabled = true;
        }
        else
        {
            // Caso contrário, apenas move a posição diretamente
            Target.transform.position = pos;
        }
        Debug.Log($"Teleportado para ({x}, {y}, {z})");
    }

    // Comando para salvar o estado do jogo para todos os jogadores registrados no DataSystem
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

    // Comando para carregar o estado salvo do jogo para todos os jogadores registrados no DataSystem
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

    // Comando para definir o alvo atual do console, baseado no playerId cadastrado no DataSystem
    [ConsoleMethod("setTarget", "Define o jogador alvo pelos dados do DataSystem", "playerId")]
    public static void SetTarget(string playerId)
    {
        // Procura o jogador pelo ID na lista de players do DataSystem
        var player = Data.players.Find(p => p.playerId == playerId);
        if (player == null)
        {
            Debug.LogWarning($"Jogador com ID '{playerId}' não encontrado.");
            return;
        }

        // Define o alvo para o GameObject associado ao jogador
        FindAnyObjectByType<ConsoleComponent>().target = player.transform.gameObject;
        Debug.Log($"Target definido para: {playerId}");
    }

    // Comando para dar um item ao inventário do jogador selecionado pelo playerId
    [ConsoleMethod("giveItem", "Adiciona um item ao inventário do jogador", "playerId itemName quantidade")]
    public static void GiveItem(string playerId, string itemName, string quantidade)
    {
        // Encontra o jogador no DataSystem
        var player = Data.players.Find(p => p.playerId == playerId);
        if (player == null)
        {
            Debug.LogWarning($"Jogador '{playerId}' não encontrado.");
            return;
        }

        // Carrega o item da pasta Resources/Items
        var item = Resources.Load<ItemDataBase>("Items/" + itemName);
        if (item == null)
        {
            Debug.LogWarning($"Item '{itemName}' não encontrado na pasta Resources/Items/");
            return;
        }

        // Converte a quantidade para int e valida
        if (!int.TryParse(quantidade, out int qtd) || qtd <= 0)
        {
            Debug.LogWarning("Quantidade inválida.");
            return;
        }

        // Adiciona o item ao inventário do jogador
        player.inventory.AddItem(item, qtd);
        Debug.Log($"Item '{itemName}' x{qtd} adicionado ao jogador '{playerId}'.");
    }

    // Comando para listar todos os jogadores registrados no DataSystem
    [ConsoleMethod("listPlayers", "Lista os IDs dos jogadores salvos no DataSystem", "")]
    public static void ListPlayers()
    {
        // Se não houver jogadores registrados, avisa no console
        if (Data.players.Count == 0)
        {
            Debug.Log("Nenhum jogador registrado no DataSystem.");
            return;
        }

        // Lista todos os IDs dos jogadores
        Debug.Log("Jogadores registrados:");
        foreach (var p in Data.players)
        {
            Debug.Log($"- {p.playerId}");
        }
    }
}
