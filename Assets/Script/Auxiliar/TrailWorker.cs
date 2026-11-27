using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailWorker : MonoBehaviour
{
  [SerializeField]
  private TrailType trailType;

  private TrailRenderer _renderer;

  public TrailType TrailType => trailType;

  private void Awake()
  {
    Cache();
    ClearTrail();
  }

  [ContextMenu("Recache")]
  private void Cache()
  {
    _renderer = GetComponent<TrailRenderer>();
  }

  public void PlayTrail()
  {
    if (!_renderer)
      Cache();

    _renderer.Clear();
    _renderer.emitting = true;
  }

  public void StopTrail()
  {
    if (!_renderer)
      Cache();

    _renderer.emitting = false;
  }

  public void ClearTrail()
  {
    if (!_renderer)
      Cache();

    _renderer.emitting = false;
    _renderer.Clear();
  }
}
