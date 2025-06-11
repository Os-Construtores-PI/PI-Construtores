using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    private List<int> offset = new() { -4,-2, 0 , 2,4 };
    private Randomizer random = new();
    private int last_numb;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform base_pos;
    public void Awake()
    {
        base_pos = transform;
    }
    public void StartSpawning()
    {
        CancelInvoke();
        InvokeRepeating(nameof(Spawn), 1, 3f);
    }

    void Spawn()
    {
        last_numb = random.NumbRandomizer(last_numb, offset.Count);
        Vector3 player = GameObject.FindWithTag("MiniYpos").transform.position;
        Vector3 pos = new(base_pos.position.x + offset[last_numb], player.y, base_pos.position.z - 5);
        GameObject clone = Instantiate(bullet, pos, Quaternion.identity);
        clone.transform.localEulerAngles = new Vector3(0,0, -90);
    }
}
