using Unity.VisualScripting;
using UnityEngine;

public class ItemDropZone : ItemComponent
{
    private GameObject visualInstance;
    private BoxCollider boxCollider;
    private Rigidbody rb;

    private void Start()
    {
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        if (itemData != null && itemData.item != null)
        {
            visualInstance = Instantiate(itemData.item, transform);
            visualInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (visualInstance.TryGetComponent<MeshRenderer>(out var mesh))
            {
                boxCollider.size = mesh.bounds.size;
                boxCollider.center = Vector3.zero;
            }
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        print("rodando");
        if (other.TryGetComponent(out InventoryComponent inventory))
        {
            if (itemData != null)
            {
                inventory.AddItem(itemData, quantity);
                Destroy(gameObject);
            }    
        }
    }
}
