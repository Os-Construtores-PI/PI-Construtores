using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ManualSingleSpawner : BasePool
{
  [SerializeField, Tooltip("Prefab to instantiate and pool.")]
  protected GameObject objectToPool;

  [HideInInspector]
  public UnityEvent<List<GameObject>> FinishedInstancing = new();

  public void Start()
  {
    PopulatePool(_initialAmount);
  }

  protected override void PopulatePool(int amount)
  {
    for (int i = 0; i < amount; i++)
    {
      var obj = Instantiate(objectToPool, _parent);
      obj.SetActive(false);
      _inactiveObjects.Add(obj);
    }

    FinishedInstancing?.Invoke(_inactiveObjects);
  }
}
