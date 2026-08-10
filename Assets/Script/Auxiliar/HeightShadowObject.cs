using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HeightShadowObject : MonoBehaviour
{
  [SerializeField]
  private DecalProjector projector;

  [SerializeField]
  private float _maxShadowSize = 1.0f; // Tamanho no chão

  [SerializeField]
  private float _minShadowSize = 0.2f; // Tamanho na altura máxima

  [SerializeField]
  private float _shadowHeightSize = 20f;

  [SerializeField]
  private float _maxDistance = 20f; // A partir daqui, a sombra não diminui mais

  public void Update()
  {
    if (
      Physics.Raycast(
        transform.position,
        Vector3.down,
        out RaycastHit hit,
        _maxDistance,
        LayerMask.GetMask("Default", "Ground")
      )
    )
    {
      float distanceNormalized = hit.distance / _maxDistance;
      float currentSize = Mathf.Lerp(_maxShadowSize, _minShadowSize, distanceNormalized);
      projector.size = new Vector3(currentSize, currentSize, _shadowHeightSize);
    }
    else
    {
      projector.size = new Vector3(_minShadowSize, _minShadowSize, _shadowHeightSize);
    }
  }
}
