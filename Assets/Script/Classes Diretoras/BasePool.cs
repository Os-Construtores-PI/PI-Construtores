using System.Collections.Generic;
using UnityEngine;

public class BasePool : MonoBehaviour
{
    public static BasePool SharedInstance;
    [HideInInspector] public List<GameObject> deactivatedObject;
    protected int amount;
    [SerializeField] protected Transform parent;

    public virtual void Awake()
    {
        SharedInstance = this;
    }

    public GameObject GetDisabledObject()
    {
        for (int i = 0; i < amount; i++)
        {
            if (!deactivatedObject[i].activeInHierarchy)
            {
                return deactivatedObject[i];
            }
        }
        return null;
    }

    public int GetAmountPool() => amount;
    public Transform GetParentTransform() => parent;
}

