using UnityEngine;
using System.Collections.Generic;

public class DrawDistance : MonoBehaviour
{
  [Header("Draw Distance")]

  [Tooltip("Distância em que o objeto aparece.")]
  [SerializeField]
  [Min(1f)]
  private float renderDistance = 275f;

  [Tooltip("Distância em que o objeto desaparece.")]
  [SerializeField]
  [Min(1f)]
  private float hideDistance = 300f;


  [Header("Opções")]
  [Tooltip("Inclui todos os objetos filhos deste GameObject.")]
  [SerializeField]
  private bool includeChildren = true;


  private class RenderObject
  {
    public Transform transform;
    public Renderer[] renderers;
    public bool isVisible;
  }


  private readonly List<RenderObject> objects = new();

  private void Awake()
  {

    if(hideDistance < renderDistance)
    {
      hideDistance = renderDistance;
    }
    
    CacheObjects();
  }


  private void CacheObjects()
  {
    objects.Clear();

    if (includeChildren)
    {
      for (int i = 0; i < transform.childCount; i++)
      {
        Transform child = 
          transform.GetChild(i);

        Renderer[] childRenders = 
          child.GetComponentsInChildren<Renderer>(true);

        if(childRenders.Length == 0)
        {
          continue;
        }

        RenderObject renderObject =
          new RenderObject
          {
            transform = child,
            renderers = childRenders,

            isVisible = true
          };

        objects.Add(renderObject);
      }
    }
    else
    {
      Renderer[] ownRenders =
        GetComponents<Renderer>();

      if (ownRenders.Length > 0)
      {
        objects.Add(
          new RenderObject
          {
            transform = transform,
            renderers = ownRenders,
            isVisible = true
          });
      }
    }
      
  }


  public float GetRenderDistance()
  {
    return renderDistance;
  }


  public float GetHideDistance()
  {
    return hideDistance;
  }



  public void UpdateObjects(Vector3 cameraPosition)
  {
    float renderDistanceSqr =
      renderDistance * renderDistance;

    float hideDistanceSqr =
      hideDistance * hideDistance;

    foreach (RenderObject renderObject in objects)
    {
      if (renderObject == null ||
        renderObject.transform == null)
      {
        continue;
      }

      Vector3 objectPostion =
        renderObject.transform.position;

      float sqrDistance =
        (cameraPosition - objectPostion).sqrMagnitude;


      if (renderObject.isVisible)
      {
        // Só desaparece depois de passar
        // da distância de esconder.
        if (sqrDistance >= hideDistanceSqr)
        {
          SetObjectVisible(
              renderObject,
              false
          );
        }
      }


      // ====================================================
      // OBJETO ESTÁ ESCONDIDO
      // ====================================================

      else
      {
        // Só volta a aparecer quando estiver
        // dentro da distância de renderização.
        if (sqrDistance <= renderDistanceSqr)
        {
          SetObjectVisible(
              renderObject,
              true
          );
        }
      }
    }
  }

  private void SetObjectVisible(RenderObject renderObject,
    bool visible)
  {
    if(renderObject.isVisible == visible)
    {
      return;
    }

    renderObject.isVisible = visible;

    foreach (Renderer renderer in renderObject.renderers)
    {
      if(renderer == null)
      {
        continue;
      }

      renderer.forceRenderingOff = !visible;
    }
  }


  public void Refresh()
  {
    CacheObjects();
  }

}
