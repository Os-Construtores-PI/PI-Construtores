using System.Collections;
using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatZone : MonoBehaviour
{
    [SerializeField] private string StatName;
    [SerializeField] private QualityTier zoneTier;
    [SerializeField] private TimeTYPE timeTYPE;
    [SerializeField] private ModifyTYPE modifyType;

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

        if (timeTYPE == TimeTYPE.TEMPORARY)
        {
            print("Funcionando");
            yield return stats.ModifyStatCoroutine<float>(StatName, ModifyTYPE.POSITIVE, zoneTier, statDuration);
        }
        else
        {
            // PERMANENTE: modificação direta sem tempo
            stats.ModifyStatImmediate<float>(StatName, ModifyTYPE.POSITIVE, zoneTier);
        }

        yield return new WaitForSeconds(statCooldown);
        _onCooldown = false;
    }
}
