using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Tools.DictionaryHelp;
using Unity.VisualScripting;

public class StatComponent : ComponentBehaviour
{
    private readonly Dictionary<StatType, float> _statModifiers = new();

    private static readonly SerializableDictionary<QualityTier, float> _tierMultipliers = new()
    {
        { QualityTier.COMMON, 1.15f },
        { QualityTier.UNCOMMON, 1.25f },
        { QualityTier.RARE, 1.30f },
        { QualityTier.EPIC, 1.45f },
        { QualityTier.LEGENDARY, 1.60f }
    };

    private bool _canApplyStat = true;

    public enum StatTime
    {
        Permanent,
        Temporary
    }

    private enum StatOperation
    {
        Positive,
        Negative
    }

    private readonly Dictionary<StatType, Func<GameObject, QualityTier, ErrorType>> _applyActions;
    private readonly Dictionary<StatType, Action<GameObject, QualityTier>> _removeActions;

    public StatComponent()
    {
        _applyActions = new Dictionary<StatType, Func<GameObject, QualityTier, ErrorType>>
        {
            { StatType.HEAL, Heal },
            { StatType.ARMOR, (target, tier) => ModifyStat<HealthComponent, float>("defense", tier, target, StatOperation.Positive) },
            { StatType.ATTACK, (target, tier) => ModifyStat<DamageComponent, float>("damage", tier, target, StatOperation.Positive) },
            { StatType.SPEED, (target, tier) => ModifyStat<PlayerMovementComponent, float>("speed", tier, target, StatOperation.Positive) },
            { StatType.JUMP, (target, tier) => ModifyStat<PlayerMovementComponent, float>("jumpForce", tier, target, StatOperation.Positive) }
        };

        _removeActions = new Dictionary<StatType, Action<GameObject, QualityTier>>
        {
            { StatType.ARMOR, (target, tier) => ModifyStat<HealthComponent, float>("defense", tier, target, StatOperation.Negative) },
            { StatType.ATTACK, (target, tier) => ModifyStat<DamageComponent, float>("damage", tier, target, StatOperation.Negative) },
            { StatType.SPEED, (target, tier) => ModifyStat<PlayerMovementComponent, float>("speed", tier, target, StatOperation.Negative) },
            { StatType.JUMP, (target, tier) => ModifyStat<PlayerMovementComponent, float>("jumpForce", tier, target, StatOperation.Negative) }
        };
    }
    public void IncreaseStat(StatType stat, QualityTier tier, GameObject target, StatTime statTime, float duration = 0f, float cooldown = 0f)
    {
        if (!_canApplyStat)
            return;

        if (!_applyActions.TryGetValue(stat, out var applyAction))
        {
            Debug.LogWarning($"Stat '{stat}' não suportado para aplicação.");
            return;
        }

        bool isReversible = _removeActions.ContainsKey(stat);
        var result = applyAction(target, tier);
        Debug.Log($"ApplyStat result: {result}");

        if (result == ErrorType.SUCCESS && statTime == StatTime.Temporary && isReversible)
        {
            if (_statModifiers.ContainsKey(stat))
            {
                _statModifiers[stat] += GetMultiplier(tier);
            }
            else
            {
                _statModifiers[stat] = GetMultiplier(tier);
            }
            _canApplyStat = false;
            StartCoroutine(HandleTemporaryStat(duration, cooldown, stat, target, tier));
        }
    }

    public void DecreaseStat(StatType stat, QualityTier tier, GameObject target)
    {
        if (!_removeActions.TryGetValue(stat, out var removeAction))
        {
            Debug.LogWarning($"Stat '{stat}' não programado para remoção.");
            return;
        }
        removeAction(target, tier);
        _statModifiers.Remove(stat);
    }

    private IEnumerator HandleTemporaryStat(float duration, float cooldown, StatType stat, GameObject target, QualityTier tier)
    {
        yield return new WaitForSeconds(duration);
        DecreaseStat(stat, tier, target);
        yield return new WaitForSeconds(cooldown);
        _canApplyStat = true;
    }

    private float GetMultiplier(QualityTier tier) => _tierMultipliers.TryGetValue(tier, out var mult) ? mult : 1f;

    private ErrorType ModifyStat<TComponent, TValue>(string attribute, QualityTier tier, GameObject target, StatOperation operation)
        where TComponent : ComponentBehaviour
        where TValue : struct, IComparable
    {
        if (!target.TryGetComponent(out TComponent component))
            return ErrorType.COMPONENT_ERROR;

        bool hasMax = component.TryGetAttribute("MAX_" + attribute, out TValue maxValue);
        TValue currentValue = component.GetAttribute<TValue>(attribute);
        float multiplier = GetMultiplier(tier);

        object newValue;

        if (typeof(TValue) == typeof(int))
        {
            int curr = Convert.ToInt32(currentValue);
            newValue = operation == StatOperation.Positive ? Mathf.RoundToInt(curr * multiplier) : Mathf.RoundToInt(curr / multiplier);
        }
        else if (typeof(TValue) == typeof(float))
        {
            float curr = Convert.ToSingle(currentValue);
            newValue = operation == StatOperation.Positive ? curr * multiplier : curr / multiplier;
        }
        else if (typeof(TValue) == typeof(bool))
        {
            newValue = operation == StatOperation.Positive;
        }
        else
        {
            Debug.LogError($"Tipo '{typeof(TValue)}' do atributo '{attribute}' não suportado.");
            return ErrorType.TYPE_ERROR;
        }
        if (hasMax)
        {
            // Clamp se tipo for int
            if (typeof(TValue) == typeof(int))
            {
                int val = Convert.ToInt32(newValue);
                int max = Convert.ToInt32(maxValue);
                newValue = Mathf.Min(val, max);
            }
            // Clamp se tipo for float 
            else if (typeof(TValue) == typeof(float))
            {
                float val = Convert.ToSingle(newValue);
                float max = Convert.ToSingle(maxValue);
                newValue = Mathf.Min(val, max);
            }
        }
        component.SetAttribute(attribute, newValue);
        return ErrorType.SUCCESS;
    }

    private ErrorType Heal(GameObject target, QualityTier tier)
    {
        if (!target.TryGetComponent(out HealthComponent health))
            return ErrorType.COMPONENT_ERROR;

        if (!health.TryGetAttribute("health", out float currentHealth))
            return ErrorType.ATTRIBUTE_ERROR;

        float healAmount = currentHealth * GetMultiplier(tier);
        health.AddHealth(healAmount);

        return ErrorType.SUCCESS;
    }
}
