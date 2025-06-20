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
        if (can_stat)
        {
            switch (newstat)
            {
                case StatType.armor:
                    //defense
                    if (target.TryGetComponent(out HealthComponent health))
                    {
                        health.SetAttribute("defense",Mathf.Clamp(health.GetAttribute<float>("defense") * EvaluateStat(tier),0,health.GetAttribute<float>("max_Defense")));
                    }
                    break;
                case StatType.attack:
                    //damage
                    if (target.TryGetComponent(out DamageComponent damage))
                    {
                        damage.SetAttribute("damage", damage.GetAttribute<float>("damage") * EvaluateStat(tier));
                    }
                    break;
                case StatType.speed:
                    //speed
                    PlayerStat("speed", tier, target,"pos");
                    break;
                case StatType.jump:
                    //jumpForce
                    PlayerStat("jumpForce", tier, target,"pos");
                    break;
                default:
                    return;
            }
            StartCoroutine(RemoveStat(statDuration, newstat, target,tier));
        }
    }
    IEnumerator RemoveStat(float duration, StatType oldstat,GameObject target, StatTier tier)
    {
        yield return new WaitForSeconds(duration);
        switch (oldstat)
        {
            case StatType.armor:
                //defense
                if (target.TryGetComponent(out HealthComponent health))
                {
                    health.SetAttribute("defense",Mathf.Clamp(health.GetAttribute<float>("defense") / EvaluateStat(tier),0,health.GetAttribute<float>("max_Defense")));
                }
                break;
            case StatType.attack:
                //damage
                if (target.TryGetComponent(out DamageComponent damage))
                {
                    damage.SetAttribute("damage", damage.GetAttribute<float>("damage") / EvaluateStat(tier));
                }
                break;
            case StatType.jump:
                //jumpForce
                PlayerStat("jumpForce", tier, target,"neg");
                break;
            case StatType.speed:
                //speed
                PlayerStat("speed", tier, target,"neg");
                break;
        }
        StartCoroutine(CooldownStat());
    }
    IEnumerator CooldownStat()
    {
        yield return new WaitForSeconds(statCooldown);
        can_stat = false;
    }
    private float EvaluateStat(StatTier tier)
    {
        Dictionary<StatTier, float> relationstat = new() { { StatTier.common, 1.15f }, { StatTier.rare, 1.30f }, { StatTier.epic, 1.45f }, { StatTier.legendary, 1.60f } };
        return relationstat[tier];
    }
    private void PlayerStat(string atributo, StatTier tier,GameObject target, string op)
    {
        if (target.TryGetComponent(out PlayerMovementComponent playerMovement))
        {
            switch (op)
            {
                case "pos":
                    playerMovement.SetAttribute(atributo, playerMovement.GetAttribute<float>(atributo) * EvaluateStat(tier));
                    break;
                case "neg":
                    playerMovement.SetAttribute(atributo, playerMovement.GetAttribute<float>(atributo) / EvaluateStat(tier));
                    break;
            }
        }
    }



}
