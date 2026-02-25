using System.Collections.Generic;
using UnityEngine;

public class BasePool : MonoBehaviour
{
  public static BasePool SharedInstance;

  [HideInInspector]
  public List<GameObject> _deactivatedObjects;

  [SerializeField]
  protected int _amount;

  [SerializeField]
  protected Transform _parent;

  public virtual void Awake()
  {
    SharedInstance = this;
  }

  public GameObject GetDisabledObject()
  {
    for (int i = 0; i < _amount; i++)
    {
      if (!_deactivatedObjects[i].activeInHierarchy)
      {
        return _deactivatedObjects[i];
      }
    }
    return null;
  }

  public List<GameObject> GetDisabledObjects()
  {
    return _deactivatedObjects;
  }

  public int GetAmountPool() => _amount;

  public Transform GetParentTransform() => _parent;
}
