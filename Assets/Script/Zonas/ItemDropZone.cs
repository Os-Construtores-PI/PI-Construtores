using UnityEngine;

// Classe que representa uma zona onde um item pode ser pego (drop zone)
public class ItemDropZone : Item
{
  private GameObject visualInstance; // Instância visual do item
  private BoxCollider boxCollider; // Colisor para detectar entrada de entidades
  private Rigidbody rb; // Rigidbody para física (kinemático)

  [SerializeField]
  protected int quantity;

#if UNITY_EDITOR
  public void OnDrawGizmos()
  {
    if (Application.isPlaying || itemData == null || itemData.item == null)
      return;

    MeshFilter mf = itemData.item.GetComponentInChildren<MeshFilter>();
    if (mf != null && mf.sharedMesh != null)
    {
      Vector3 visualScale = itemData.item.transform.localScale;

      Gizmos.color = Color.white;
      Gizmos.DrawWireMesh(mf.sharedMesh, transform.position, transform.rotation, visualScale);
    }
  }

  public void OnValidate()
  {
    // Força a atualização do Scene View quando você altera variáveis no Inspetor
    UnityEditor.SceneView.RepaintAll();
  }
#endif

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
      if (itemData.item.TryGetComponent<MeshRenderer>(out var mesh))
      {
        boxCollider.size = mesh.bounds.size;
        boxCollider.center = Vector3.zero;
      }
    }
  }

  public override void Start()
  {
    base.Start();
    Initialize();
  }

  // Método chamado quando outra colisão entra no trigger
  public void OnTriggerEnter(Collider other)
  {
    if (other.TryGetComponent(out Player player))
    {
      if (itemData != null)
      {
        AddItem(player);
      }
    }
  }

  protected virtual void AddItem(Player player) { }
}
