using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class MiniPool : MonoBehaviour
{
    [SerializeField] protected MiniGameControl gameControl;
    [SerializeField] protected List<GameObject> pooledObjects;
    [SerializeField] protected GameObject objectToPool;
    public int amountToPool;
    [SerializeField] protected GameObject father;


    public virtual void Awake()
    {
        gameControl = GameObject.FindWithTag("MiniGameController").GetComponent<MiniGameControl>();
    }

    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool, father.transform);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
    }
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
