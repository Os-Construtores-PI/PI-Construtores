using UnityEngine;

public class ManualSpawner : BasePool
{
    void Start()
    {
        Instance();
    }
    protected void Instance()
    {
        disabledObject = new();
        GameObject tmp;
        for (int i = 0; i < amount; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            disabledObject.Add(tmp);
        }
    }
}
