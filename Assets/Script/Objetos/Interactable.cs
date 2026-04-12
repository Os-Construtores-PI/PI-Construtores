using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractableObject : MonoBehaviour
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

  public bool IsActive => enabled && gameObject.activeInHierarchy;

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
    Debug.Log($"[GetCorrentSprite] _ultimoDispositivo do player = {player._ultimoDispositivo}");

    return player._ultimoDispositivo switch
    {
      InputType.Keyboard => _keyboardSprites,
      InputType.JoystickPlaystation => _playstationSprites,
      InputType.JoystickXbox => _xboxSprites,
      _ => _keyboardSprites,
    };
  }
}
