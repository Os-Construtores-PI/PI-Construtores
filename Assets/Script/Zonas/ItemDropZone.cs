using UnityEngine;
using UnityEngine.Events;

public class ItemDropZone : Item
{
  #region Fields & Events
  [Header("Visual")]
  [SerializeField]
  private float _visualScaleMultiplier = 1f;

  [SerializeField]
  protected bool _destroyOnCollect = true;

  [Header("Colisão")]
  [SerializeField]
  private Vector3 _colliderSizeMultiplier = new(1.5f, 1.2f, 1.5f);

  [SerializeField]
  private Vector3 _colliderOffset = Vector3.zero;

  [Header("Quantidade")]
  [SerializeField]
  protected int quantity = 1;

  public UnityEvent<ItemData, int, Player> OnItemCollected;

  private GameObject _visualInstance;
  protected BoxCollider _boxCollider;
  private bool _isCollected = false;
  #endregion

  #region Unity Lifecycle
#if UNITY_EDITOR
  private void OnDrawGizmosSelected()
  {
    if (itemData == null || itemData.item == null)
      return;

    Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.7f);

    if (_boxCollider != null)
    {
      Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
      Gizmos.DrawWireCube(_boxCollider.center + _colliderOffset, _boxCollider.size);
    }
    else if (Application.isPlaying)
    {
      MeshFilter mf = itemData.item.GetComponentInChildren<MeshFilter>();
      if (mf?.sharedMesh != null)
      {
        Vector3 scale = itemData.item.transform.localScale * _visualScaleMultiplier;
        Gizmos.DrawWireMesh(mf.sharedMesh, transform.position, transform.rotation, scale);
      }
    }
  }

  private void OnValidate()
  {
    _colliderSizeMultiplier = Vector3.Max(_colliderSizeMultiplier, Vector3.one * 0.1f);
  }
#endif

  public override void Awake()
  {
    base.Awake();
    Initialize();
  }

  public override void Start()
  {
    base.Start();
    if (_boxCollider == null)
      Initialize();
  }
  #endregion

  #region Initialization
  public virtual void Initialize()
  {
    SetupCollider();
    InstantiateVisual();
  }

  private void SetupCollider()
  {
    if (_boxCollider == null)
    {
      _boxCollider = gameObject.AddComponent<BoxCollider>();
      _boxCollider.isTrigger = true;
    }

    if (itemData?.item != null)
    {
      MeshRenderer meshRenderer = itemData.item.GetComponentInChildren<MeshRenderer>();
      if (meshRenderer != null)
      {
        Bounds bounds = meshRenderer.bounds;
        Vector3 localSize = transform.InverseTransformVector(bounds.size);
        _boxCollider.size = Vector3.Scale(localSize, _colliderSizeMultiplier);
        _boxCollider.center = _colliderOffset;
        return;
      }
    }

    _boxCollider.size = Vector3.one * 2f;
    _boxCollider.center = _colliderOffset;
  }

  private void InstantiateVisual()
  {
    if (itemData?.item == null || transform.childCount > 0)
      return;

    _visualInstance = Instantiate(itemData.item, transform);
    _visualInstance.name = $"Visual_{itemData.item.name}";
    _visualInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    _visualInstance.transform.localScale = Vector3.one * _visualScaleMultiplier;

    foreach (var rb in _visualInstance.GetComponentsInChildren<Rigidbody>())
      rb.isKinematic = true;
  }
  #endregion

  #region Collection Logic
  private void OnTriggerEnter(Collider other)
  {
    if (_isCollected)
      return;

    if (other.TryGetComponent(out Player player))
      TryCollect(player);
  }

  protected virtual bool TryCollect(Player player)
  {
    if (player == null || itemData == null)
      return false;

    if (!CanCollect(player))
      return false;

    _isCollected = true;
    AddItem(player);
    OnItemCollected?.Invoke(itemData, quantity, player);
    AfterCollect();

    return true;
  }

  // Ponto de extensão: subclasses customizam o ciclo de vida pós-coleta aqui.
  // O comportamento padrão usa _destroyOnCollect.
  protected virtual void AfterCollect()
  {
    if (_destroyOnCollect)
      Destroy(gameObject);
    else
      DisableZone();
  }

  protected virtual bool CanCollect(Player player) => true;

  protected virtual void AddItem(Player player)
  {
    if (player.Inventory != null)
    {
      player.Inventory.AddItem(itemData, quantity);
    }
    else
    {
      Debug.LogWarning(
        $"[ItemDropZone] Player '{player.name}' não possui Inventory configurado.",
        player
      );
    }
  }

  protected virtual void DisableZone()
  {
    _isCollected = true;
    enabled = false;
    if (_boxCollider != null)
      _boxCollider.enabled = false;
    if (_visualInstance != null)
      _visualInstance.SetActive(false);
  }

  public virtual void ResetZone()
  {
    _isCollected = false;
    enabled = true;
    if (_boxCollider != null)
      _boxCollider.enabled = true;
    if (_visualInstance != null)
      _visualInstance.SetActive(true);
  }
  #endregion

  #region Public API
  public void SetQuantity(int newQuantity) => quantity = Mathf.Max(1, newQuantity);

  public bool IsCollected => _isCollected;
  #endregion
}
