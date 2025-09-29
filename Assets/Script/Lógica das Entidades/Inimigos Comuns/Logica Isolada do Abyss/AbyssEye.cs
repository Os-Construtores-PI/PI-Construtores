using System.Collections;
using UnityEngine;

public class AbyssEye : Enemies
{
    public override void DeathHandler()
    {
        base.DeathHandler();
        print("MORREU");
        gameObject.SetActive(false);
    }
}
