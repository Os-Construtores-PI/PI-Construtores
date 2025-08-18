using Unity.VisualScripting;
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
        // Tenta pegar o componente de inventário da entidade que entrou
        // if (other.TryGetComponent(out InventoryComponent inventory))
        // {
        //     // Tenta pegar o BrainComponent para identificar o tipo da entidade (se existir)
        //     if (other.TryGetComponent(out BrainComponent brain))
        //     {
        //         // Verifica se o tipo da entidade está na lista permitida
        //         if (allowedEntityTypes.Length > 0 && !System.Array.Exists(allowedEntityTypes, t => t == brain.identity.TipoEntidade))
        //         {
        //             // Se o tipo não está na lista, não permite pegar
        //             return;
        //         }
        //     }
        //     // Se passou na checagem ou não tem BrainComponent, adiciona o item ao inventário
        //     if (itemData != null)
        //     {
        //         inventory.AddItem(itemData, quantity);
        //         Destroy(gameObject); // Remove a zona de drop após o item ser pego
        //     }
        // }
    }
}
