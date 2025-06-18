using System.Collections;
using UnityEngine;

public class StatComponent : EntityBehavior
{
    [SerializeField] int stat_duration;
    [SerializeField] int stat_cooldown;

    public void ApplyStat(StatType newstat)
    {
        // ...
        StartCoroutine(RemoveStat(stat_duration, newstat));
    }
    IEnumerator RemoveStat(int duration, StatType oldstat)
    {
        yield return new WaitForSeconds(duration);
        // ...
    }
}
