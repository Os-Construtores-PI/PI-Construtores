using System;
using System.Collections;
using UnityEngine;

public class StatZone : MonoBehaviour
{
  [SerializeField]
  private Constants.StatsNames StatName;

  [SerializeField]
  private QualityTier zoneTier;

  [SerializeField]
  private TimeTYPE timeTYPE;

  [SerializeField]
  private ModifyTYPE modifyType;

  [Header("Só funciona se for status temporário")]
  [SerializeField]
  private float statDuration = 5f;

  [SerializeField]
  private float statCooldown = 10f;

  private bool _onCooldown = false;

  private void OnTriggerEnter(Collider other)
  {
    if (_onCooldown)
      return;
    if (other.gameObject.layer == LayerMask.NameToLayer("Entity"))
    {
      if (other.TryGetComponent(out CombatEntities combatentity))
      {
        Stats stats = combatentity.Stats;
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
    Type statType = StatTypeMap.Map[StatName];

    if (timeTYPE == TimeTYPE.TEMPORARY)
    {
      print($"Funcionando // {StatName}");

      var method = typeof(Stats)
        .GetMethod(nameof(Stats.ModifyStatCoroutine))
        .MakeGenericMethod(statType);

      yield return (IEnumerator)
        method.Invoke(
          stats,
          new object[] { StatName.ToString(), modifyType, zoneTier, statDuration }
        );
    }
    else
    {
      var method = typeof(Stats)
        .GetMethod(nameof(Stats.ModifyStatImmediate))
        .MakeGenericMethod(statType);

      method.Invoke(stats, new object[] { StatName.ToString(), modifyType, zoneTier });
    }

    if (timeTYPE == TimeTYPE.TEMPORARY)
    {
      print($"Funcionando // {StatName.ToString()}");
      yield return stats.ModifyStatCoroutine<bool>(
        StatName.ToString(),
        modifyType,
        zoneTier,
        statDuration
      );
    }
    else
    {
      stats.ModifyStatImmediate<float>(StatName.ToString(), modifyType, zoneTier);
    }

    yield return new WaitForSeconds(statCooldown);
    _onCooldown = false;
  }
}
