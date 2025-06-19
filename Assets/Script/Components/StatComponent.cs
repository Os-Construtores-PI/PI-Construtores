using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatComponent : ComponentBehaviour
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
