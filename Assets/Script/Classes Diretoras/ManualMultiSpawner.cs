using System.Collections.Generic;
using UnityEngine;

public class ManualPlayersSpawner : BasePool
{
    protected List<GameObject> gameObjectsToPool = new();
    public void SetObjects(List<GameObject> gameObjects)
    {
        gameObjectsToPool = gameObjects;    
    }
    private void Start()
    {
        Instance();
    }
    protected void Instance()
    {
        disabledObject = new();
        GameObject tmp;
        for (int i = 0; i < gameObjectsToPool.Count; i++)
        {
            tmp = Instantiate(gameObjectsToPool[i]);
            tmp.SetActive(false);
            disabledObject.Add(tmp);
        }
    }
}
