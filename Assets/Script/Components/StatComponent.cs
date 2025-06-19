using System.Collections;
using UnityEngine;

public class StatComponent : ComponentBehaviour
{
    [SerializeField] int StatDuration;
    [SerializeField] int StatCooldown;
    private void Start()
    {
        SetAttribute(nameof(StatDuration), StatDuration);
        SetAttribute(nameof(StatCooldown), StatCooldown);
        SubscribeToAttribute(nameof(StatDuration), (newDuration) =>
        {
            print("UpdateUI");
         });
    }

    public void ApplyStat(StatType newstat)
    {
        // ...
        StartCoroutine(RemoveStat(StatDuration, newstat));
    }
    IEnumerator RemoveStat(int duration, StatType oldstat)
    {
        yield return new WaitForSeconds(duration);
        // ...
    }
}
