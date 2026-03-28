using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ManualSingleSpawner : BasePool
{
    [SerializeField]
    protected GameObject objectToPool;
    public UnityEvent<List<GameObject>> FinishedInstancing = new();

    void Start()
    {
        Instance();
    }

    protected void Instance()
    {
        _deactivatedObjects = new();
        GameObject tmp;
        for (int i = 0; i < _amount; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.transform.SetParent(_parent);
            tmp.SetActive(false);
            _deactivatedObjects.Add(tmp);
        }
        FinishedInstancing.Invoke(_deactivatedObjects);
    }
}
