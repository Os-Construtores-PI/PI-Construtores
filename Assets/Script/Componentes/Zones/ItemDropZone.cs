using UnityEngine;

// Classe que representa uma zona onde um item pode ser pego (drop zone)
public class ItemDropZone : ItemComponent
{
    private GameObject visualInstance;    // Instância visual do item
    private BoxCollider boxCollider;      // Colisor para detectar entrada de entidades
    private Rigidbody rb;                 // Rigidbody para física (kinemático)

    [Header("Tipos de Entidade que podem pegar o item")]
    public CombatEntities[] allowedEntityTypes;  // Lista de entidades autorizadas

    public void Initialize()
    {
        // Adiciona BoxCollider configurado como trigger para detectar colisões sem bloqueio físico
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        if (rb == null)
        {
            // Rigidbody kinemático para interagir com física sem ser afetado por forças
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }


        // Instancia o modelo visual do item se definido no ScriptableObject
        if (itemData != null && itemData.item != null && transform.childCount < 1)
        {
            visualInstance = Instantiate(itemData.item, transform);
            visualInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Ajusta o tamanho do colisor baseado no mesh render do modelo visual
            if (visualInstance.TryGetComponent<MeshRenderer>(out var mesh))
            {
                boxCollider.size = mesh.bounds.size;
                boxCollider.center = Vector3.zero;
            }
        }
    }
    private void Start()
    {
        Initialize();
    }
    // Método chamado quando outra colisão entra no trigger
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (itemData != null)
            {
                player.Inventario.AddItem(itemData, quantity);
                Destroy(gameObject); // Remove a zona de drop após o item ser pego
            }
        }
    }
}
