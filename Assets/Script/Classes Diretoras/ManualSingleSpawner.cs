using System.Collections.Generic;
using UnityEngine;

public class ManualSingleSpawner : BasePool
{
    [SerializeField] protected GameObject objectToPool;

    void Start()
    {
        Instance();
    }
    protected void Instance()
    {
        deactivatedObject = new();
        GameObject tmp;
        for (int i = 0; i < amount; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            deactivatedObject.Add(tmp);
        }
    }
}
