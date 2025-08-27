using UnityEngine;
using IngameDebugConsole;
using System;
using System.Collections.Generic;

// Componente que registra comandos para o Ingame Debug Console
public class ConsoleComponent : MonoBehaviour
{

    private static readonly Dictionary<TypeCode, Action<LiveEntities, string, ModifyTYPE, QualityTier, float, TimeTYPE>> statModifiers =
    new()
    {
        {TypeCode.Single,ModifyFloatStat},
        {TypeCode.Boolean,ModifyBoolStat}
    };

    // Referência pública para o alvo dos comandos (ex: o jogador selecionado)
    public GameObject target;

    // Propriedade estática que retorna o sistema de dados ativo na cena (gerencia save/load e jogadores)
    private static DataSystem Data => FindAnyObjectByType<DataSystem>();

    // Propriedade estática que retorna o GameObject alvo atual do console
    private static GameObject Target => FindAnyObjectByType<ConsoleComponent>().target;
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
    [ConsoleMethod("saveGame", "Salva o estado do jogo para todos os jogadores", "slot")]
    public static void SaveGame(int index)
    {
        var dataSystem = FindAnyObjectByType<DataSystem>();
        if (dataSystem == null)
        {
            Debug.LogError("DataSystem não encontrado na cena.");
            return;
        }

        dataSystem.Save(index);
    }

    // Comando para carregar o estado salvo do jogo para todos os jogadores registrados no DataSystem
    [ConsoleMethod("loadGame", "Carrega o estado do jogo para todos os jogadores", "slot")]
    public static void LoadGame(int index)
    {
        var dataSystem = FindAnyObjectByType<DataSystem>();
        if (dataSystem == null)
        {
            Debug.LogError("DataSystem não encontrado na cena.");
            return;
        }

        dataSystem.Load(index);
    }

    // Comando para definir o alvo atual do console, baseado no playerId cadastrado no DataSystem
    [ConsoleMethod("setTarget", "Define o jogador alvo pelos dados do DataSystem", "playerId")]
    public static void SetTarget(int playerId)
    {
        // Procura o jogador pelo ID na lista de players do DataSystem
        var player = Data.players.Find(p => p.ID == playerId);
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
    public static void GiveItem(int playerId, string itemName, string quantidade)
    {
        // Encontra o jogador no DataSystem
        var player = Data.players.Find(p => p.ID == playerId);
        if (player == null)
        {
            Debug.LogWarning($"Jogador '{playerId}' não encontrado.");
            return;
        }

        // Carrega o item da pasta Resources/Items
        var item = Resources.Load<ItemData>("Items/" + itemName);
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
        player.Inventario.AddItem(item, qtd);
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
            Debug.Log($"- {p.ID}");
        }
    }


    [ConsoleMethod("modifyStat", "Modifica os status do target", "TIME NOME_STATUS TIPO_STATUS POS_OU_NEG QUALITYTIER DURAÇÃO")]
    public static void ModifyStat(TimeTYPE time, string statname, string type, ModifyTYPE modtype, QualityTier tier, float duration = 5.0f)
    {
        print(type);
        if (Target == null || !Target.TryGetComponent(out LiveEntities live))
        {
            print("Sem Target ou sem LiveEntities");
            return;
        }
        TypeCode typeCode = Type.GetTypeCode(StringtoTypes.TypeMap[type.ToLower()]);
        if (statModifiers.TryGetValue(typeCode, out var action))
        {
            action.Invoke(live, statname, modtype, tier, duration, time);
        }
        else
        {
            Debug.LogWarning($"Tipo '{type}' não suportado.");
        }
    }






    private static void ModifyFloatStat(LiveEntities live, string statName, ModifyTYPE modType, QualityTier tier, float duration, TimeTYPE timeType)
    {
        if (timeType == TimeTYPE.PERMANENT)
            live.stats.ModifyStatImmediate<float>(statName, modType, tier);
        else
            live.StartCoroutine(live.stats.ModifyStatCoroutine<float>(statName, modType, tier, duration));
    }

    private static void ModifyBoolStat(LiveEntities live, string statName, ModifyTYPE modType, QualityTier tier, float duration, TimeTYPE timeType)
    {
        if (timeType == TimeTYPE.PERMANENT)
            live.stats.ModifyStatImmediate<bool>(statName, modType, tier);
        else
            live.StartCoroutine(live.stats.ModifyStatCoroutine<bool>(statName, modType, tier, duration));
    }
}

