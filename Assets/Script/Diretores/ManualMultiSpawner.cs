using System.Collections.Generic;
using UnityEngine;

public class ManualPlayersSpawner : BasePool
{
  [SerializeField]
  private Transform spawnPosition;

  protected override void PopulatePool(int amount)
  {
    _inactiveObjects.Clear();
    _activeObjects.Clear();
  }

  public void SetObjects(List<GameObject> gameObjects)
  {
    _inactiveObjects.Clear();
    _activeObjects.Clear();

    if (gameObjects == null || gameObjects.Count == 0)
      return;

    for (int i = 0; i < gameObjects.Count; i++)
    {
      var obj = Instantiate(gameObjects[i], _parent);
      obj.SetActive(false);
      _inactiveObjects.Add(obj);
    }
  }

  public GameObject Spawn(int index)
  {
    if (index < 0 || index >= _inactiveObjects.Count)
      return null;

    var obj = _inactiveObjects[index];
    _inactiveObjects.RemoveAt(index);
    _activeObjects.Add(obj);

    // 🔍 Debug
    Debug.Log(
      $"[Spawn] Antes: {obj.transform.position} | spawnPosition: {(spawnPosition ? spawnPosition.position.ToString() : "NULL")}"
    );

    if (spawnPosition != null)
      obj.transform.position = spawnPosition.position;

    obj.SetActive(true);

    Debug.Log($"[Spawn] Depois: {obj.transform.position}");

    return obj;
  }

  public void ReturnToPool(int index)
  {
    if (index < 0 || index >= _activeObjects.Count)
      return;

    var obj = _activeObjects[index];
    _activeObjects.RemoveAt(index);
    _inactiveObjects.Add(obj);

    obj.SetActive(false);
  }

  public GameObject GetDeactivatedObject(int index)
  {
    if (index < 0 || index >= _inactiveObjects.Count)
      return null;
    return _inactiveObjects[index];
  }

  public int DeactivatedObjectsCount => _inactiveObjects.Count;
  public int ActiveObjectsCount => _activeObjects.Count;

  public Transform GetSpawnPosition() => spawnPosition;
}
