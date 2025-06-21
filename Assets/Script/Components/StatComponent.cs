using System;
using System.Collections;
using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatComponent : ComponentBehaviour
{
    [SerializeField] float statDuration;
    [SerializeField] float statCooldown;
    private readonly Dictionary<StatType, float> statModifiers = new();

    public Dictionary<StatType, float> GetStatBonuses() => new(statModifiers);


    static readonly SerializableDictionary<QualityTier, float> relationstat = new() {
        { QualityTier.COMMON, 1.15f },
        { QualityTier.RARE, 1.30f },
        { QualityTier.EPIC, 1.45f },
        { QualityTier.LEGENDARY, 1.60f } };

    private bool can_stat = true;

    public enum StatTime
    {
        PERMANENT,TEMPORARY
    }

    private void Start()
    {
        SetAttribute(nameof(statDuration), statDuration);
        SetAttribute(nameof(statCooldown), statCooldown);
    }

    public void ApplyStat(StatType newstat, QualityTier tier, GameObject target, StatTime statTime)
    {
        ErrorType status_code;
        if (can_stat)
        {
            switch (newstat)
            {
                case StatType.ARMOR:
                    //defense
                    status_code = Stat<HealthComponent, float>("defense", tier, target, "pos");
                    break;
                case StatType.ATTACK:
                    //damage
                    status_code = Stat<DamageComponent, float>("damage", tier, target, "pos");
                    break;
                case StatType.SPEED:
                    //speed
                    status_code = Stat<PlayerMovementComponent, float>("speed", tier, target, "pos");
                    break;
                case StatType.JUMP:
                    //jumpForce
                    status_code = Stat<PlayerMovementComponent, float>("jumpForce", tier, target, "pos");
                    break;
                default:
                    return;
            }
            print($"ApplyStat_debugCode: {status_code}");
            if (status_code == ErrorType.SUCCESS && statTime == StatTime.TEMPORARY)
            {
                can_stat = false;
                StartCoroutine(RemoveTempStat(statDuration, newstat, target, tier));
            }
        }
    }
    public void RemoveStat(StatType stat, QualityTier tier, GameObject target)
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
    IEnumerator RemoveTempStat(float duration, StatType oldstat, GameObject target, QualityTier tier)
    {
        StartCoroutine(CooldownStat(statCooldown));
        yield return new WaitForSeconds(duration);
        switch (oldstat)
        {
            case StatType.ARMOR:
                //defense
                Stat<HealthComponent, float>("defense", tier, target, "neg");
                break;
            case StatType.ATTACK:
                //damage
                Stat<DamageComponent,float>("damage", tier, target, "neg");
                break;
            case StatType.JUMP:
                //jumpForce
                Stat<PlayerMovementComponent,float>("jumpForce", tier, target, "neg");
                break;
            case StatType.SPEED:
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
    private float EvaluateStat(QualityTier tier)
    {
        return relationstat[tier];
    }
    private ErrorType Stat<TComponent, TValue>(string atributo, QualityTier tier, GameObject target, string op)
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
