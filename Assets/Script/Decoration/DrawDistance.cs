using UnityEngine;

public class DrawDistance : MonoBehaviour
{
  [Header("Draw Distance")]
  [SerializeField]
  [Min(1f)]
  private float drawDistance = 80f;

  [Header("Opções")]
  [SerializeField] private bool incluideChildren = true;

  private Renderer[] renders;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    CacheRenderers();
  }

  private void CacheRenderers()
  {
    if (incluideChildren)
    {
      renders = GetComponentsInChildren<Renderer>(true);
    }
    else
    {
        renders = GetComponentsInChildren<Renderer>();
    }
  }

  public float GetDrawDistance()
  {
    return drawDistance;
  }

  public Renderer[] GetRenderers()
  {
    return renders;
  }

}
