using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractableObject : Object
{
  [Header("Sprites de Interação")]
  public Sprite _keyboardSprites; //F
  public Sprite _playstationSprites; //X
  public Sprite _xboxSprites; //A

  [Header("Opções de Interação")]
  [SerializeField]
  public float Range = 10;

  [Header("Opções de Cooldown")]
  [SerializeField]
  protected float _interactionCooldown = 1f;
  protected readonly Timer _interactionTimer = new();

  public virtual bool IsActive => true;

  public virtual void Interaction(Player info) { }

#if UNITY_EDITOR
  private Collider _interactionCollider;

  public virtual void OnDrawGizmos()
  {
    _interactionCollider = GetComponent<Collider>();
    if (_interactionCollider != null)
    {
      Gizmos.DrawWireCube(_interactionCollider.bounds.center, _interactionCollider.bounds.size);
    }
  }
#endif

  public virtual Sprite GetCurrentSprite(Player player)
  {
    if (player == null)
      return _keyboardSprites;
    Debug.Log($"[GetCorrentSprite] _ultimoDispositivo do player = {player.LastDevice}");

    return player.LastDevice switch
    {
      DeviceType.Keyboard => _keyboardSprites,
      DeviceType.Playstation => _playstationSprites,
      DeviceType.Xbox => _xboxSprites,
      _ => _keyboardSprites,
    };
  }
}
