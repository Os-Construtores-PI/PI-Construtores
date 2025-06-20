using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Rendering;
using UnityEngine;

public class StatComponent : ComponentBehaviour
{
    [SerializeField] float statDuration;
    [SerializeField] float statCooldown;


    private bool can_stat = true;
    public enum StatType
    {
        armor, attack, speed, jump
    }
    public enum StatTier
    {
        common, rare, epic, legendary
    }


    private void Start()
    {
        SetAttribute(nameof(statDuration), statDuration);
        SetAttribute(nameof(statCooldown), statCooldown);
    }

    public void ApplyStat(StatType newstat, StatTier tier, GameObject target)
    {
        ErrorType status_code;
        if (can_stat)
        {
            switch (newstat)
            {
                case StatType.armor:
                    //defense
                    status_code = Stat<HealthComponent, float>("defense", tier, target, "pos");
                    break;
                case StatType.attack:
                    //damage
                    status_code = Stat<DamageComponent, float>("damage", tier, target, "pos");
                    break;
                case StatType.speed:
                    //speed
                    status_code = Stat<PlayerMovementComponent, float>("speed", tier, target, "pos");
                    break;
                case StatType.jump:
                    //jumpForce
                    status_code = Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "pos");
                    break;
                default:
                    return;
            }
            print($"ApplyStat_debugCode: {status_code}");
            if (status_code == ErrorType.SUCCESS)
            {
                can_stat = false;
                StartCoroutine(RemoveStat(statDuration, newstat, target, tier));
            }
        }
    }
    IEnumerator RemoveStat(float duration, StatType oldstat, GameObject target, StatTier tier)
    {
        StartCoroutine(CooldownStat(statCooldown));
        yield return new WaitForSeconds(duration);
        switch (oldstat)
        {
            case StatType.armor:
                //defense
                Stat<HealthComponent, float>("defense", tier, target, "neg");
                break;
            case StatType.attack:
                //damage
                Stat<DamageComponent,float>("damage", tier, target, "neg");
                break;
            case StatType.jump:
                //jumpForce
                Stat<PlayerMovementComponent,float>("jumpForce", tier, target, "neg");
                break;
            case StatType.speed:
                //speed
                Stat<PlayerMovementComponent,float>("speed", tier, target, "neg");
                break;
        }
    }
    IEnumerator CooldownStat(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        can_stat = true;
    }
    private float EvaluateStat(StatTier tier)
    {
        Dictionary<StatTier, float> relationstat = new() { { StatTier.common, 1.15f }, { StatTier.rare, 1.30f }, { StatTier.epic, 1.45f }, { StatTier.legendary, 1.60f } };
        return relationstat[tier];
    }
    private ErrorType Stat<TComponent, TValue>(string atributo, StatTier tier, GameObject target, string op)
    where TComponent : ComponentBehaviour
    where TValue : struct, IComparable
    {
        if (target.TryGetComponent(out TComponent component))
        {
            bool hasMaxValue = component.TryGetAttribute("MAX_" + atributo, out TValue maxValue);
            TValue currentValue = component.GetAttribute<TValue>(atributo);
            float statMultiplier = EvaluateStat(tier);
            TValue newValue = op == "pos"
            ? Operators<TValue>.Multiply(currentValue, statMultiplier)
            : Operators<TValue>.Divide(currentValue, statMultiplier);
            if (hasMaxValue)
            {
                newValue = Operators<TValue>.Clamp(newValue,default, maxValue);
            }
            component.SetAttribute(atributo, newValue);
            return ErrorType.SUCCESS;
        }
        return ErrorType.COMPONENT_ERROR;
    }



}
