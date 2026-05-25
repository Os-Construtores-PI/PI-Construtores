using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ManualSingleSpawner))]
public class PhysicsCannonObject : BaseRenderedGameObject
{
  [Header("Configurações do Canhão")]
  [SerializeField, Min(0)]
  private float _initialExplosionForce = 10f;

  [SerializeField, Min(0)]
  private float _shootCooldown = 2f;

  [SerializeField]
  private bool _autoBehavior = true;

  [SerializeField]
  private ManualSingleSpawner _objectSpawner;

  [Header("Configurações de despawn")]
  [SerializeField, Min(0)]
  private float _timeToDespawn = 5f;

  [Header("Feedback")]
  [SerializeField]
  private AudioClip _shootSound;

  [SerializeField]
  private ParticleSystem _muzzleFlash;

  [Header("Debug")]
  [SerializeField]
  private bool _drawGizmos = true;

  [SerializeField]
  private Color _gizmoColor = Color.white;

  public UnityEvent<GameObject> OnObjectShot = new();
  public UnityEvent OnPoolExhausted = new();

  private readonly Timer _shootCooldownTimer = new();
  private bool _isInitialized;

  public override void Start()
  {
    base.Start();
    Setup();
    _isInitialized = true;

    if (_autoBehavior)
    {
      _shootCooldownTimer.Start(_shootCooldown);
      Shoot();
    }
  }

  public void Update()
  {
    if (!_autoBehavior || !enabled)
      return;

    if (_shootCooldownTimer.Tick(Time.deltaTime))
    {
      Shoot();
      _shootCooldownTimer.Start(_shootCooldown);
    }
  }

  public void OnDrawGizmos()
  {
    if (!_drawGizmos)
      return;
    Gizmos.color = _gizmoColor;
    Gizmos.DrawLine(transform.position, transform.position + transform.up * _initialExplosionForce);
  }

  private void Setup()
  {
    if (_objectSpawner == null)
      _objectSpawner = GetComponent<ManualSingleSpawner>();
  }

  public void Shoot()
  {
    GameObject projectile = _objectSpawner?.AcquireObject();
    if (projectile == null)
    {
      OnPoolExhausted?.Invoke();
      Debug.LogWarning($"[{gameObject.name}] Pool esgotado! Considere aumentar o tamanho do pool.");
      return;
    }

    if (!projectile.TryGetComponent(out TemporizedDespawnObject despawn))
    {
      Debug.LogError($"[{gameObject.name}] Projetil não possui TemporizedDespawnObject");
      _objectSpawner.ReturnObject(projectile);
      return;
    }

    if (!projectile.TryGetComponent(out Rigidbody rb))
    {
      Debug.LogError($"[{gameObject.name}] Projetil não possui Rigidbody");
      _objectSpawner.ReturnObject(projectile);
      return;
    }

    projectile.transform.SetPositionAndRotation(transform.position, transform.rotation);
    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.AddForce(transform.up * _initialExplosionForce, ForceMode.Impulse);

    despawn.Initialize(_timeToDespawn, _objectSpawner);

    PlayFeedback();
    OnObjectShot?.Invoke(projectile);
  }

  private void PlayFeedback()
  {
    if (_muzzleFlash != null && !_muzzleFlash.isPlaying)
    {
      _muzzleFlash.Play();
    }
    if (_shootSound != null)
    {
      AudioSource.PlayClipAtPoint(_shootSound, transform.position);
    }
  }

  public void ToggleAutoShoot(bool enabled)
  {
    _autoBehavior = enabled;
    this.enabled = enabled;
  }

  public void ForceShoot()
  {
    Shoot();
    _shootCooldownTimer.Start(_shootCooldown);
  }

  public void SetShootCooldown(float seconds)
  {
    _shootCooldown = Mathf.Max(0, seconds);
    if (_shootCooldownTimer != null && _isInitialized)
    {
      _shootCooldownTimer.Start(_shootCooldown);
    }
  }

  public void ResetCooldown()
  {
    _shootCooldownTimer.Start(_shootCooldown);
  }
}
