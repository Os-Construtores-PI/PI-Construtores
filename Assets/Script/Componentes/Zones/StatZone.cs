using System;
using System.Collections;
using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatZone : MonoBehaviour
{
    [SerializeField] private Constants.StatsNames StatName;
    [SerializeField] private QualityTier zoneTier;
    [SerializeField] private TimeTYPE timeTYPE;
    [SerializeField] private ModifyTYPE modifyType;
    [SerializeField] private string statType;

    [Header("Só funciona se for status temporário")]
    [SerializeField] private float statDuration = 5f;
    [SerializeField] private float statCooldown = 10f;

    private bool _onCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_onCooldown) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            if (other.TryGetComponent(out CombatEntities combatentity))
            {
                Stats stats = combatentity.stats;
                if (stats != null)
                {
                    StartCoroutine(ApplyStatZone(stats));
                }
            }
        }
    }

    private IEnumerator ApplyStatZone(Stats stats)
    {
        _onCooldown = true;

        switch (statType)
        {
            case "bool":
                if (timeTYPE == TimeTYPE.TEMPORARY)
                {
                    print($"Funcionando // {StatName}");
                    yield return stats.ModifyStatCoroutine<bool>(
                        StatName.ToString(), modifyType, zoneTier, statDuration
                    );
                }
                else
                {
                    stats.ModifyStatImmediate<bool>(
                        StatName.ToString(), modifyType, zoneTier
                    );
                }
                break;

            case "float":
                if (timeTYPE == TimeTYPE.TEMPORARY)
                {
                    print($"Funcionando // {StatName}");
                    yield return stats.ModifyStatCoroutine<float>(
                        StatName.ToString(), modifyType, zoneTier, statDuration
                    );
                }
                else
                {
                    stats.ModifyStatImmediate<float>(
                        StatName.ToString(), modifyType, zoneTier
                    );
                }
                break;

            default:
                yield break;
        }


        if (timeTYPE == TimeTYPE.TEMPORARY)
        {
            print($"Funcionando // {StatName.ToString()}");
            yield return stats.ModifyStatCoroutine<bool>(StatName.ToString(), modifyType, zoneTier, statDuration);
        }
        else
        {
            stats.ModifyStatImmediate<float>(StatName.ToString(), modifyType, zoneTier);
        }

        yield return new WaitForSeconds(statCooldown);
        _onCooldown = false;
    }
}
