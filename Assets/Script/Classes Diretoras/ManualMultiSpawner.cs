using System.Collections.Generic;
using UnityEngine;

public class ManualPlayersSpawner : BasePool
{
    [SerializeField] private Transform spawnPosition;

    public void SetObjects(List<GameObject> gameObjects)
    {
        deactivatedObjects = new();
        amount = gameObjects.Count;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject tmp = Instantiate(gameObjects[i], parent);
            tmp.SetActive(false);
            deactivatedObjects.Add(tmp);
        }
    }

    public GameObject Spawn(int index)
    {
        if (index < 0 || index >= deactivatedObjects.Count) return null;

        GameObject obj = deactivatedObjects[index];
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
        if (index < 0 || index >= deactivatedObjects.Count) return null;
        return deactivatedObjects[index];
    }


public int DeactivatedObjectsCount => deactivatedObjects.Count;

    public Transform GetSpawnPosition() => spawnPosition;
}


