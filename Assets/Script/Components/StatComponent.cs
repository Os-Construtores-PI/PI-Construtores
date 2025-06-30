using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatComponent : ComponentBehaviour
{
    // Dicionário para armazenar modificadores aplicados (não utilizado diretamente neste código)
    private readonly Dictionary<StatType, float> statModifiers = new();

    // Multiplicadores baseados na raridade do status (QualityTier)
    static readonly SerializableDictionary<QualityTier, float> relationstat = new()
    {
        { QualityTier.COMMON, 1.15f },
        { QualityTier.UNCOMMON, 1.25f },
        { QualityTier.RARE, 1.30f },
        { QualityTier.EPIC, 1.45f },
        { QualityTier.LEGENDARY, 1.60f }
    };

    private bool can_stat = true; // Flag que controla se é possível aplicar status (usado para cooldown)

    // Define se o efeito é permanente ou temporário
    public enum StatTime
    {
        PERMANENT,
        TEMPORARY
    }

    // Dicionário que mapeia o StatType para a função que aplica o efeito positivo
    private readonly Dictionary<StatType, Func<GameObject, QualityTier, ErrorType>> applyStatActions;

    // Dicionário que mapeia o StatType para a função que remove o efeito (negativo)
    private readonly Dictionary<StatType, Action<GameObject, QualityTier>> removeStatActions;

    // Construtor: Inicializa os dicionários com as funções apropriadas
    public StatComponent()
    {
        applyStatActions = new Dictionary<StatType, Func<GameObject, QualityTier, ErrorType>>
        {
            { StatType.HEAL, Heal },
            { StatType.ARMOR, (target, tier) => Stat<HealthComponent, float>("defense", tier, target, "pos") },
            { StatType.ATTACK, (target, tier) => Stat<DamageComponent, float>("damage", tier, target, "pos") },
            { StatType.SPEED, (target, tier) => Stat<PlayerMovementComponent, float>("speed", tier, target, "pos") },
            { StatType.JUMP, (target, tier) => Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "pos") }
        };

        removeStatActions = new Dictionary<StatType, Action<GameObject, QualityTier>>
        {
            { StatType.ARMOR, (target, tier) => Stat<HealthComponent, float>("defense", tier, target, "neg") },
            { StatType.ATTACK, (target, tier) => Stat<DamageComponent, float>("damage", tier, target, "neg") },
            { StatType.SPEED, (target, tier) => Stat<PlayerMovementComponent, float>("speed", tier, target, "neg") },
            { StatType.JUMP, (target, tier) => Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "neg") }
        };
    }

    /// <summary>
    /// Aplica um status positivo no alvo, podendo ser permanente ou temporário.
    /// Se temporário, inicia corrotina para remover após duração e aplicar cooldown.
    /// </summary>
    public void IncreaseStat(StatType newstat, QualityTier tier, GameObject target, StatTime statTime, float duration = 0, float cooldown = 0)
    {
        if (!can_stat) return; // Se estiver em cooldown, não aplica

        if (!applyStatActions.TryGetValue(newstat, out var applyAction))
            return; // Status não suportado

        bool reversivel = removeStatActions.ContainsKey(newstat); // Verifica se pode ser revertido

        ErrorType status_code = applyAction.Invoke(target, tier); // Aplica o efeito
        print($"ApplyStat_debugCode: {status_code}");

        // Se sucesso e temporário, inicia corrotina para remover após duração e aplicar cooldown
        if (status_code == ErrorType.SUCCESS && statTime == StatTime.TEMPORARY && reversivel)
        {
            can_stat = false; // Bloqueia aplicação de outros status temporários enquanto estiver em cooldown
            StartCoroutine(RemoveTempStat(duration, cooldown, newstat, target, tier));
        }
    }

    /// <summary>
    /// Remove um status do alvo (efeito negativo).
    /// </summary>
    public void DecreaseStat(StatType stat, QualityTier tier, GameObject target)
    {
        if (!removeStatActions.TryGetValue(stat, out var removeAction))
        {
            print("STATUS NÃO PROGRAMADO");
            return;
        }
        removeAction.Invoke(target, tier);
    }

    /// <summary>
    /// Corrotina que aguarda a duração do efeito, depois remove o status e inicia cooldown.
    /// </summary>
    IEnumerator RemoveTempStat(float duration, float cooldown, StatType oldstat, GameObject target, QualityTier tier)
    {
        StartCoroutine(CooldownStat(cooldown)); // Inicia cooldown
        yield return new WaitForSeconds(duration); // Aguarda duração

        if (!removeStatActions.TryGetValue(oldstat, out var removeAction))
        {
            print("STATUS NÃO PROGRAMADO");
            yield break;
        }
        removeAction.Invoke(target, tier); // Remove o status temporário
    }

    /// <summary>
    /// Corrotina que mantém o cooldown ativo para impedir aplicação de novos status temporários.
    /// </summary>
    IEnumerator CooldownStat(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        can_stat = true; // Libera aplicação de status após cooldown
    }

    /// <summary>
    /// Retorna o multiplicador baseado no QualityTier.
    /// </summary>
    private float EvaluateStat(QualityTier tier) => relationstat[tier];

    /// <summary>
    /// Aplica ou remove modificadores nos atributos genéricos dos componentes usando reflexão genérica.
    /// "op" indica se é operação positiva ("pos") ou negativa ("neg").
    /// </summary>
    private ErrorType Stat<TComponent, TValue>(string atributo, QualityTier tier, GameObject target, string op)
        where TComponent : ComponentBehaviour
        where TValue : struct, IComparable
    {
        if (!target.TryGetComponent(out TComponent component))
            return ErrorType.COMPONENT_ERROR;

        bool hasMaxValue = component.TryGetAttribute("MAX_" + atributo, out TValue maxValue);
        TValue currentValue = component.GetAttribute<TValue>(atributo);
        float statMultiplier = EvaluateStat(tier);

        object newVal;

        if (typeof(TValue) == typeof(int))
        {
            int curr = Convert.ToInt32(currentValue);
            newVal = op == "pos" ? Mathf.RoundToInt(curr * statMultiplier) : Mathf.RoundToInt(curr / statMultiplier);
        }
        else if (typeof(TValue) == typeof(float))
        {
            float curr = Convert.ToSingle(currentValue);
            newVal = op == "pos" ? curr * statMultiplier : curr / statMultiplier;
        }
        else if (typeof(TValue) == typeof(bool))
        {
            newVal = op == "pos";
        }
        else
        {
            Debug.LogError($"Tipo de atributo '{typeof(TValue)}' não suportado.");
            return ErrorType.TYPE_ERROR;
        }

        component.SetAttribute(atributo, newVal);
        return ErrorType.SUCCESS;
    }

    /// <summary>
    /// Aplica cura direta na entidade alvo, baseada na raridade.
    /// </summary>
    private ErrorType Heal(GameObject target, QualityTier tier)
    {
        if (!target.TryGetComponent(out HealthComponent health))
            return ErrorType.COMPONENT_ERROR;

        if (!health.TryGetAttribute("health", out float health_value))
            return ErrorType.ATTRIBUTE_ERROR;

        health.AddHealth(health_value * EvaluateStat(tier));
        return ErrorType.SUCCESS;
    }
}
