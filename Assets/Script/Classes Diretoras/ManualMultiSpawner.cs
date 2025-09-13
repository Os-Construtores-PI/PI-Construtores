using System.Collections.Generic;
using UnityEngine;

public class ManualPlayersSpawner : BasePool
{
    [SerializeField] private Transform spawnPosition;

    public void SetObjects(List<GameObject> gameObjects)
    {
        deactivatedObject = new();
        amount = gameObjects.Count;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject tmp = Instantiate(gameObjects[i], parent);
            tmp.SetActive(false);
            deactivatedObject.Add(tmp);
        }
    }

    public GameObject Spawn(int index)
    {
        if (index < 0 || index >= deactivatedObject.Count) return null;

        GameObject obj = deactivatedObject[index];
        obj.SetActive(true);
        if (spawnPosition != null)
            obj.transform.position = spawnPosition.position;
        return obj;
    }

    public Transform GetSpawnPosition() => spawnPosition;
}

