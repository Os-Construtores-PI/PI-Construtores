using System.Collections.Generic;
using UnityEngine;

public abstract class BasePool : MonoBehaviour
{
  #region Serialized Fields

  [SerializeField, Tooltip("Number of objects to pre-warm the pool with.")]
  protected int _initialAmount = 10;

  [SerializeField, Tooltip("Parent transform to organize pooled objects under.")]
  protected Transform _parent;

  #endregion

  #region Internal State

  protected List<GameObject> _inactiveObjects = new List<GameObject>();
  protected List<GameObject> _activeObjects = new List<GameObject>();

  public IReadOnlyList<GameObject> InactiveObjects => _inactiveObjects.AsReadOnly();
  public IReadOnlyList<GameObject> ActiveObjects => _activeObjects.AsReadOnly();

  #endregion

  #region Unity Lifecycle

  protected virtual void Awake()
  {
    EnsureParentExists();
    WarmUpPool();
  }

  #endregion

  #region Pool Management

  protected virtual void WarmUpPool()
  {
    _inactiveObjects.Clear();
    _activeObjects.Clear();
  }

  protected abstract void PopulatePool(int count);

  public virtual GameObject GetDisabledObject()
  {
    for (int i = _inactiveObjects.Count - 1; i >= 0; i--)
    {
      var obj = _inactiveObjects[i];
      if (obj != null && !obj.activeSelf)
        return obj;
    }
    return null;
  }

  public virtual GameObject AcquireObject()
  {
    for (int i = _inactiveObjects.Count - 1; i >= 0; i--)
    {
      var obj = _inactiveObjects[i];
      if (obj != null)
      {
        _inactiveObjects.RemoveAt(i);
        _activeObjects.Add(obj);
        obj.SetActive(true);
        return obj;
      }
    }
    return null;
  }

  public virtual void ReturnObject(GameObject obj)
  {
    if (obj == null)
      return;

    obj.SetActive(false);
    obj.transform.SetParent(_parent, worldPositionStays: false);

    if (_activeObjects.Remove(obj) && !_inactiveObjects.Contains(obj))
    {
      _inactiveObjects.Add(obj);
    }
  }

  #endregion

  #region Utilities

  public int GetAmountPool() => _initialAmount;

  public Transform GetParentTransform() => _parent;

  private void EnsureParentExists()
  {
    if (_parent == null)
      _parent = new GameObject($"[{GetType().Name}_Pool]").transform;
  }

  #endregion
}
