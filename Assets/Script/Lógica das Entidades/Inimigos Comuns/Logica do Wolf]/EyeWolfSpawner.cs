using UnityEngine;

public class EyeWolfSpawner : MonoBehaviour
{

    public GameObject _wolfPrefab;
    public int _minPackSize = 3;
    public int _maxPackSize = 5;


    public void SpawnPack(Vector3 position)
    {
        int packSize = Random.Range(_minPackSize, _maxPackSize);

        for (int i = 0; i < packSize; i++)
        {
            Vector3 spawnPos = position + Random.insideUnitSphere * 2f;
            spawnPos.y = 0; // chao

            Instantiate(_wolfPrefab, spawnPos, Quaternion.identity);
        }
    }  
}
