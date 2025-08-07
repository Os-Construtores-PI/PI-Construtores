using UnityEngine;

public class CollectibleAmethyst : Collectible
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    private void OnTriggerEnter(Collider other) // For 3D
    {
        if (other.CompareTag("Player"))
        {
            // Call a method on a central manager to update the counter
            //CollectableManager.Instance.AddCollectable();
            Destroy(gameObject);
        }
    }
}
