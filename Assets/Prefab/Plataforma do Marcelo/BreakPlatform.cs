using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class BreakPlatform : MonoBehaviour
{

    // script que faz a animação de cair a plataforma

    //script01
    public Rigidbody[] pieces;
    public float delay = 0.5f;


    public float delayBetweenPieces = 0.2f;
    public float startDelay = 0.3f;


    private bool activated = false;
    private bool activated2 = false;

    //Script01
    //[Header("Configurações de Spawn")]
    //public List<GameObject> prefabsParaCair; // Lista de prefabs
    //public float tempoSpawn = 1.0f; // Tempo entre quedas
    //public float areaX = 5.0f; // Largura da área de queda
    //public float alturaSpawn = 0.0f; // Altura da queda

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Script01
        // Chama a função spawnRepeatedly a cada tempoSpawn segundos
        //InvokeRepeating("SpawnPrefab", 0.0f, tempoSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            if (activated2 == false)
            {
                activated2 = true;
                StartCoroutine(DropPieces());
            }          
        }
    }

    void ActivatePhysics()
    {
        foreach (Rigidbody rb in pieces)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(200f, transform.position, 5f);
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }
        StartCoroutine(DropPieces());
        Invoke(nameof(ActivatePhysics), delay);
    }

    IEnumerator DropPieces()
    {
        foreach (Rigidbody rb in pieces)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.GetComponent<Collider>().enabled = false;
                yield return new WaitForSeconds(delayBetweenPieces);
                Destroy(rb.gameObject, 3f);
            }
            else
            {
                StopCoroutine(DropPieces());
                
            }

        }
    }

    //Script01
   // void SpawnPrefab()
    //{
    //    if (prefabsParaCair.Count == 0) return;

        // Escolhe um prefab aleatório da lista
     //   int index = Random.Range(0, prefabsParaCair.Count);
     //   GameObject prefabSelecionado = prefabsParaCair[index];

        // Define posição aleatória no topo
        //Vector3 posicaoSpawn = new Vector3(Random.Range(-areaX, areaX), alturaSpawn, Random.Range(-areaX, areaX));
        
        // Instancia o prefab
        //Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
    //}

}
