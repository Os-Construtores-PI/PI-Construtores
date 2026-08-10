using UnityEngine;

public class TemporizedDespawnObject : MonoBehaviour
{
  [SerializeField, Min(0)]
  private float _timeToDespawn = 5f;

  private readonly Timer _despawnTimer = new();
  private bool _isActive;
  private ManualSingleSpawner _spawnerOwner;

  public float TimeToDespawn
  {
    get => _timeToDespawn;
    set => _timeToDespawn = Mathf.Max(0, value);
  }

  public void Initialize(float timeToDespawn, ManualSingleSpawner spawner)
  {
    _timeToDespawn = Mathf.Max(0, timeToDespawn);
    _spawnerOwner = spawner;
    _isActive = true;
    _despawnTimer.Start(_timeToDespawn);
    enabled = true;
  }

  public void OnEnable()
  {
    if (_isActive && _timeToDespawn > 0 && _despawnTimer != null)
    {
      _despawnTimer.Start(_timeToDespawn);
    }
  }

  public void OnDisable()
  {
    _isActive = false;
  }

  public void Update()
  {
    if (!_isActive || _spawnerOwner == null)
      return;

    if (_despawnTimer.Tick(Time.deltaTime))
    {
      ReturnToPool();
    }
  }

  private void ReturnToPool()
  {
    _isActive = false;
    enabled = false;

    if (_spawnerOwner != null)
    {
      _spawnerOwner.ReturnObject(gameObject);
    }
    else
    {
      gameObject.SetActive(false);
      Debug.LogWarning(
        $"[{name}] SpawnerOwner não definido. Objeto desativado mas não retornado ao pool."
      );
    }
  }

  public void ExtendDespawnTime(float extraSeconds)
  {
    if (!_isActive || extraSeconds <= 0)
      return;

    var remaining = _timeToDespawn - _despawnTimer.TimeLeft;
    _timeToDespawn = Mathf.Max(0, remaining + extraSeconds);
    _despawnTimer.Start(_timeToDespawn);
  }

  public void CancelDespawn()
  {
    _isActive = false;
    enabled = false;
  }

  public float GetRemainingTime()
  {
    if (!_isActive)
      return 0;
    return Mathf.Max(0, _timeToDespawn - _despawnTimer.TimeLeft);
  }
}
