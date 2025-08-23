using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : BasePool
{
    public static EnemySpawner enemySpawner;
    [SerializeField] protected List<Spawner> spawners;

    public override void Awake()
    {
        base.Awake();
        enemySpawner = this;
    }
    private void Start()
    {
        InitSpawner();
        SetupInstance();
    }
    protected void SetupInstance()
    {
        disabledObject = new();
        GameObject tmp;
        foreach (Spawner sp in spawners)
        {
            amount = sp.positions.Count;
            for (int i = 0; i < amount; i++)
            {
                Transform tmpMarker = sp.positions[i];
                tmp = Instantiate(sp.obj, tmpMarker.position, tmpMarker.rotation, parent);
                if (tmp.TryGetComponent(out Enemies enemy))
                {
                    enemy.spawnpos = tmpMarker.position;
                }
                tmp.SetActive(false);
                disabledObject.Add(tmp);
            }
        }
    }
    private void InitSpawner()
    {
        foreach (Spawner sp in spawners)
        {
            GameObject[] tempposarray = GameObject.FindGameObjectsWithTag(sp.spawner_tag);
            foreach (GameObject tempos in tempposarray)
            {
                sp.positions.Add(tempos.transform);
            }
        }
    }
}
