using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : BasePool
{
  [SerializeField]
  protected List<Spawner> spawners;

  public static EnemySpawner Instance;

  protected override void Awake()
  {
    base.Awake();
    Instance = this;
    InitSpawners();
  }

  private void InitSpawners()
  {
    if (spawners == null)
      return;
    foreach (var sp in spawners)
    {
      sp.positions.Clear();
      GameObject[] found = GameObject.FindGameObjectsWithTag(sp.spawner_tag);
      foreach (var go in found)
      {
        sp.positions.Add(go.transform);
      }
    }
  }

  protected override void PopulatePool(int amount)
  {
    _inactiveObjects.Clear();
    _activeObjects.Clear();

    foreach (var sp in spawners)
    {
      if (sp.obj == null)
        continue;

      foreach (var marker in sp.positions)
      {
        var enemy = Instantiate(sp.obj, marker.position, marker.rotation, _parent);
        if (enemy.TryGetComponent(out Enemies e))
        {
          e.spawnpos = marker.position;
        }
        enemy.SetActive(false);
        _inactiveObjects.Add(enemy);
      }
    }
  }

  public GameObject Spawn(int index)
  {
    if (index < 0 || index >= _inactiveObjects.Count)
      return null;

    var obj = _inactiveObjects[index];
    _inactiveObjects.RemoveAt(index);
    _activeObjects.Add(obj);

    obj.SetActive(true);
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
}
