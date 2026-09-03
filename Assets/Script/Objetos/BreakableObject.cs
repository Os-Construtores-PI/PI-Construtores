using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class BreakableObject : MonoBehaviour, IRespawnable
{
  private static readonly int ResetHash = Animator.StringToHash("Reset");

  [SerializeField]
  private int _amethystsAmount = 10;

  [Header("Animation")]
  [SerializeField]
  private Animator _animator;

  [SerializeField]
  private string _breakAnimationName = "Break";

  [SerializeField]
  private GameObject _realModel;

  [SerializeField]
  private GameObject _partsModel;

  public bool IsAlive { get; private set; } = true;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
    GameDirector.RespawnManager.Register(this);
  }

  private void OnDestroy()
  {
    GameDirector.RespawnManager.Unregister(this);
  }

  private void OnTriggerEnter(Collider collider)
  {
    if (!IsAlive)
      return;

    if (collider.TryGetComponent(out Player player))
    {
      Break(player);
    }
  }

  private void Break(Player player)
  {
    _realModel.SetActive(false);
    _partsModel.SetActive(true);

    player.AddAmethysts(_amethystsAmount);

    _animator.Play(_breakAnimationName);

    float animationDuration = GetAnimationDuration(_breakAnimationName);
    DOVirtual.DelayedCall(
      animationDuration,
      () =>
      {
        IsAlive = false;
        gameObject.SetActive(false);
        _partsModel.SetActive(false);
      }
    );
  }

  public void Respawn()
  {
    IsAlive = true;
    gameObject.SetActive(true);
    _realModel.SetActive(true);
  }

  private float GetAnimationDuration(string animationName)
  {
    if (_animator.runtimeAnimatorController == null)
      return 1f;

    foreach (var clip in _animator.runtimeAnimatorController.animationClips)
    {
      if (clip.name == animationName)
        return clip.length;
    }

    return 1f;
  }
}
