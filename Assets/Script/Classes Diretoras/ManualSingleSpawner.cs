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
        disabledObject = new();
        GameObject tmp;
        for (int i = 0; i < amount; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            disabledObject.Add(tmp);
        }
    }
}
