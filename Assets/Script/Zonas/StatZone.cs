using System;
using System.Collections;
using UnityEngine;

public class StatZone : MonoBehaviour
{
  [SerializeField]
  private StatType StatType;

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
    Type statType = StatTypeMap.Map[StatType];

    if (timeTYPE == TimeTYPE.TEMPORARY)
    {
      print($"Funcionando // {StatType}");

      var method = typeof(Stats)
        .GetMethod(nameof(Stats.ModifyStatCoroutine))
        .MakeGenericMethod(statType);

      yield return (IEnumerator)
        method.Invoke(stats, new object[] { StatType, modifyType, zoneTier, statDuration });
    }
    else
    {
      var method = typeof(Stats)
        .GetMethod(nameof(Stats.ModifyStatImmediate))
        .MakeGenericMethod(statType);

      method.Invoke(stats, new object[] { StatType, modifyType, zoneTier });
    }

    if (timeTYPE == TimeTYPE.TEMPORARY)
    {
      print($"Funcionando // {StatType}");
      yield return stats.ModifyStatCoroutine<bool>(StatType, modifyType, zoneTier, statDuration);
    }
    else
    {
      stats.ModifyStatImmediate<float>(StatType, modifyType, zoneTier);
    }

    yield return new WaitForSeconds(statCooldown);
    _onCooldown = false;
  }
}
