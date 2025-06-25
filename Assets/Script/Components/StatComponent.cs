using System;
using System.Collections;
using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatComponent : ComponentBehaviour
{
    // Dicionário que guarda modificadores de status aplicados (não utilizado diretamente no código atual)
    private readonly Dictionary<StatType, float> statModifiers = new();

    // Dicionário serializável que define multiplicadores baseados na raridade (tier) da estatística
    static readonly SerializableDictionary<QualityTier, float> relationstat = new() {
        { QualityTier.COMMON, 1.15f },
        { QualityTier.UNCOMMON,1.25f},
        { QualityTier.RARE, 1.30f },
        { QualityTier.EPIC, 1.45f },
        { QualityTier.LEGENDARY, 1.60f }
    };

    // Flag que controla se é possível aplicar um novo status (evita sobreposição durante cooldown)
    private bool can_stat = true;

    // Enum para diferenciar se o efeito do status é permanente ou temporário
    public enum StatTime
    {
        PERMANENT,
        TEMPORARY
    }

    // Método principal para aplicar um modificador de status a um alvo
    // Pode ser permanente ou temporário (com duração e cooldown)
    public void IncreaseStat(StatType newstat, QualityTier tier, GameObject target, StatTime statTime, float duration=0, float cooldown=0)
    {
        ErrorType status_code;

        // Só aplica o status se estiver disponível (não estiver em cooldown)
        if (can_stat)
        {
            // Dependendo do tipo de status, chama o método genérico Stat para modificar o atributo correto no componente alvo
            switch (newstat)
            {
                case StatType.ARMOR:
                    // Modifica defesa no HealthComponent
                    status_code = Stat<HealthComponent, float>("defense", tier, target, "pos");
                    break;
                case StatType.ATTACK:
                    // Modifica dano no DamageComponent
                    status_code = Stat<DamageComponent, float>("damage", tier, target, "pos");
                    break;
                case StatType.SPEED:
                    // Modifica velocidade no PlayerMovementComponent
                    status_code = Stat<PlayerMovementComponent, float>("speed", tier, target, "pos");
                    break;
                case StatType.JUMP:
                    // Modifica força do pulo no PlayerMovementComponent
                    status_code = Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "pos");
                    break;
                default:
                    return; // Status não reconhecido, não faz nada
            }

            print($"ApplyStat_debugCode: {status_code}");

            // Se aplicado com sucesso e é temporário, inicia coroutine para remover o efeito após duração e cooldown
            if (status_code == ErrorType.SUCCESS && statTime == StatTime.TEMPORARY)
            {
                can_stat = false; // Bloqueia aplicação de novos stats temporários
                StartCoroutine(RemoveTempStat(duration, cooldown, newstat, target, tier));
            }
        }
    }

    // Método para remover um status específico do alvo (usar efeito inverso)
    public void DecreaseStat(StatType stat, QualityTier tier, GameObject target)
    {
        switch (stat)
        {
            case StatType.ARMOR:
                Stat<HealthComponent, float>("defense", tier, target, "neg");
                break;
            case StatType.ATTACK:
                Stat<DamageComponent, float>("damage", tier, target, "neg");
                break;
            case StatType.SPEED:
                Stat<PlayerMovementComponent, float>("speed", tier, target, "neg");
                break;
            case StatType.JUMP:
                Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "neg");
                break;
        }
    }

    // Coroutine para aguardar a duração do efeito temporário, remover o status, e depois iniciar cooldown
    IEnumerator RemoveTempStat(float duration, float cooldown, StatType oldstat, GameObject target, QualityTier tier)
    {
        StartCoroutine(CooldownStat(cooldown));  // Inicia cooldown paralelamente
        yield return new WaitForSeconds(duration); // Espera o tempo de duração do efeito

        // Remove o efeito do status aplicado (inverso da aplicação)
        switch (oldstat)
        {
            case StatType.ARMOR:
                Stat<HealthComponent, float>("defense", tier, target, "neg");
                break;
            case StatType.ATTACK:
                Stat<DamageComponent, float>("damage", tier, target, "neg");
                break;
            case StatType.JUMP:
                Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "neg");
                break;
            case StatType.SPEED:
                Stat<PlayerMovementComponent, float>("speed", tier, target, "neg");
                break;
        }
    }

    // Coroutine que aguarda o cooldown para permitir nova aplicação de status temporários
    IEnumerator CooldownStat(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        can_stat = true; // Libera para aplicar novamente
    }

    // Avalia o multiplicador do status baseado na raridade do QualityTier
    private float EvaluateStat(QualityTier tier)
    {
        return relationstat[tier];
    }

    // Método genérico que aplica ou remove um modificador de status no componente alvo
    // TComponent é o tipo de componente alvo, TValue o tipo do atributo a modificar (int, float ou bool)
    // "atributo" é o nome do atributo a ser alterado (ex: "defense", "damage")
    // "op" indica operação: "pos" para aplicar multiplicador, "neg" para reverter
    private ErrorType Stat<TComponent, TValue>(string atributo, QualityTier tier, GameObject target, string op)
        where TComponent : ComponentBehaviour
        where TValue : struct, IComparable
    {
        // Tenta pegar o componente do alvo
        if (target.TryGetComponent(out TComponent component))
        {
            // Verifica se existe valor máximo para o atributo (ex: MAX_defense)
            bool hasMaxValue = component.TryGetAttribute("MAX_" + atributo, out TValue maxValue);

            // Pega o valor atual do atributo
            TValue currentValue = component.GetAttribute<TValue>(atributo);

            // Pega multiplicador baseado na raridade
            float statMultiplier = EvaluateStat(tier);

            object newVal;

            // Modifica o valor baseado no tipo do atributo
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
                // Para booleanos, simplesmente define true ou false dependendo da operação
                newVal = op == "pos";
            }
            else
            {
                Debug.LogError($"Tipo de atributo '{typeof(TValue)}' não suportado.");
                return ErrorType.TYPE_ERROR;
            }

            // Aplica o novo valor ao componente
            component.SetAttribute(atributo, newVal);
            return ErrorType.SUCCESS;
        }

        // Retorna erro caso não encontre o componente
        return ErrorType.COMPONENT_ERROR;
    }
}
