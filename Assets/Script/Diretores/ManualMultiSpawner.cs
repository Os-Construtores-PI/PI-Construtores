using System.Collections.Generic;
using UnityEngine;

public class ManualPlayersSpawner : BasePool
{
  [SerializeField]
  private Transform spawnPosition;

  public void SetObjects(List<GameObject> gameObjects)
  {
    _deactivatedObjects = new();
    _amount = gameObjects.Count;

    for (int i = 0; i < gameObjects.Count; i++)
    {
      GameObject tmp = Instantiate(gameObjects[i], _parent);
      tmp.SetActive(false);
      _deactivatedObjects.Add(tmp);
    }
  }

  public GameObject Spawn(int index)
  {
    if (index < 0 || index >= _deactivatedObjects.Count)
      return null;

    GameObject obj = _deactivatedObjects[index];
    obj.SetActive(true);
    if (spawnPosition != null)
    {
      obj.transform.position = spawnPosition.position;
      Physics.SyncTransforms();
    }
    return obj;
  }

  public GameObject GetDeactivatedObject(int index)
  {
    if (index < 0 || index >= _deactivatedObjects.Count)
      return null;
    return _deactivatedObjects[index];
  }

  public int DeactivatedObjectsCount => _deactivatedObjects.Count;

  public Transform GetSpawnPosition() => spawnPosition;
}
