using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class DrawDistance : MonoBehaviour
{
  [Header("Draw Distance")]

  [Tooltip(
    "Distância a partir da qual o GameObject será desativado."
  )]
  [SerializeField]
  [Min(1f)]
  private float hideDistance = 160f;


  [Tooltip(
    "Distância na qual o GameObject será ativado novamente."
  )]
  [SerializeField]
  [Min(1f)]
  private float renderDistance = 80f;


  [Header("GameObject")]

  [Tooltip(
    "GameObject que será ativado ou desativado."
  )]
  [SerializeField]
  private GameObject targetObject;


  [Header("Debug")]

  [SerializeField]
  private bool debugMode = false;


  private bool wasHiddenByDrawDistance = false;


  // ============================================================
  // UPDATE DRAW DISTANCE
  // ============================================================

  public void UpdateDrawDistance(Vector3 playerPosition)
  {
    if (targetObject == null)
      return;

    float distanceSqr =
      (playerPosition - targetObject.transform.position).sqrMagnitude;

    float hideDistanceSqr =
      hideDistance * hideDistance;

    float renderDistanceSqr =
      renderDistance * renderDistance;


    if(distanceSqr >= hideDistanceSqr)
    {
      if (!wasHiddenByDrawDistance)
      {
        targetObject.SetActive(false);

        wasHiddenByDrawDistance = true;

        if (debugMode)
        {
          Debug.Log(
            $"[DrawDistance] {targetObject.name} -> DESATIVADO");
        }
      }
       return;
    }


    if(distanceSqr <= renderDistanceSqr)
    {
      if (wasHiddenByDrawDistance)
      {
        targetObject.SetActive(true);

        wasHiddenByDrawDistance = false;

        if (debugMode)
        {
          Debug.Log(
            $"[DrawDistance] {targetObject.name} -> ATIVADO");
        }
      }

      return;
    }



  }


  // ============================================================
  // TARGET
  // ============================================================

  public void SetTarget(GameObject newTarget)
  {
    targetObject = newTarget;
  }


  // ============================================================
  // REFRESH
  // ============================================================

  public void Refresh(Vector3 playerPosition)
  {
    UpdateDrawDistance(playerPosition);
  }


}
