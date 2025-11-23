using UnityEngine;

public class Collectible : MonoBehaviour
{

    [Header("Efeito Especial")]
    public AudioClip _collentSound;
    public ParticleSystem _collectEffect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectibleManager.Instance.AddColletable(1);

            if (_collentSound)
                AudioSource.PlayClipAtPoint(_collentSound, transform.position);
            if (_collectEffect)
                Instantiate(_collectEffect, transform.position, Quaternion.identity);
            
            Destroy(gameObject);
        }
    }
}
