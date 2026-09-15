using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class DrawDistance : MonoBehaviour
{
  [Header("Draw Distance")]

  [Tooltip(
      "Distância a partir da qual o Renderer será escondido."
  )]
  [SerializeField]
  [Min(1f)]
  private float hideDistance = 160f;


  [Header("Layer")]

  [Tooltip(
      "Somente Renderers pertencentes às Layers selecionadas " +
      "serão controlados."
  )]
  [SerializeField]
  private LayerMask targetLayers = ~0;


  [Header("Opções")]

  [Tooltip(
      "Procura Renderers em todos os filhos."
  )]
  [SerializeField]
  private bool includeChildren = true;


  [Tooltip(
      "Intervalo entre as verificações de distância."
  )]
  [SerializeField]
  [Min(0.05f)]
  private float updateInterval = 0.15f;


  [Header("Player")]

  [Tooltip(
      "Player usado como referência. " +
      "Se vazio, será encontrado automaticamente."
  )]
  [SerializeField]
  private Player playerTarget;


  [Header("Debug")]

  [SerializeField]
  private bool debugMode = false;


  // ============================================================
  // DADOS
  // ============================================================

  private Renderer[] targetRenderers;

  private float updateTimer;


  // ============================================================
  // AWAKE
  // ============================================================

  private void Awake()
  {
    FindRenderers();
  }


  // ============================================================
  // START
  // ============================================================

  private void Start()
  {
    FindPlayer();

    /*
     * Faz uma verificação imediatamente.
     */
    if (playerTarget != null)
    {
      UpdateDrawDistance();
    }
  }


  // ============================================================
  // UPDATE
  // ============================================================

  private void Update()
  {
    updateTimer += Time.deltaTime;


    if (updateTimer < updateInterval)
    {
      return;
    }


    updateTimer = 0f;


    // ----------------------------------------------------------
    // PLAYER
    // ----------------------------------------------------------

    if (playerTarget == null)
    {
      FindPlayer();


      if (playerTarget == null)
      {
        return;
      }
    }


    // ----------------------------------------------------------
    // DRAW DISTANCE
    // ----------------------------------------------------------

    UpdateDrawDistance();
  }


  // ============================================================
  // ENCONTRA PLAYER
  // ============================================================

  private void FindPlayer()
  {
    if (playerTarget != null)
    {
      return;
    }


    playerTarget =
        FindFirstObjectByType<Player>();


    if (
        playerTarget != null &&
        debugMode
    )
    {
      Debug.Log(
          $"[DrawDistance] {name} encontrou Player: " +
          $"{playerTarget.name}"
      );
    }
  }


  // ============================================================
  // ENCONTRA RENDERERS
  // ============================================================

  private void FindRenderers()
  {
    if (includeChildren)
    {
      targetRenderers =
          GetComponentsInChildren<Renderer>(true);
    }
    else
    {
      targetRenderers =
          GetComponents<Renderer>();
    }


    if (
        targetRenderers == null ||
        targetRenderers.Length == 0
    )
    {
      Debug.LogWarning(
          $"[DrawDistance] {name} não encontrou " +
          $"nenhum Renderer."
      );

      return;
    }


    int validRenderers = 0;


    foreach (Renderer renderer in targetRenderers)
    {
      if (renderer == null)
      {
        continue;
      }


      /*
       * A Layer é verificada no GameObject
       * que realmente possui o Renderer.
       *
       * Isso é importante para:
       *
       * Tentaculo
       *   └── Circle
       *        └── SkinnedMeshRenderer
       *
       * Se Circle estiver em Mobile_Culling,
       * ele será controlado.
       */

      if (!IsLayerAllowed(
          renderer.gameObject.layer))
      {
        continue;
      }


      validRenderers++;


      if (debugMode)
      {
        Debug.Log(
            $"[DrawDistance] Renderer encontrado: " +
            $"{renderer.name} | " +
            $"Layer: {LayerMask.LayerToName(renderer.gameObject.layer)}"
        );
      }

    }


    if (debugMode)
    {
      Debug.Log(
          $"[DrawDistance] {name} encontrou " +
          $"{validRenderers} Renderer(s) " +
          $"controláveis."
      );
    }
  }


  // ============================================================
  // VERIFICA LAYER
  // ============================================================

  private bool IsLayerAllowed(int layer)
  {
    return
        (targetLayers.value & (1 << layer)) != 0;
  }


  // ============================================================
  // DRAW DISTANCE
  // ============================================================

  private void UpdateDrawDistance()
  {
    if (
        targetRenderers == null ||
        targetRenderers.Length == 0
    )
    {
      return;
    }


    Vector3 playerPosition =
        playerTarget.transform.position;


    float hideDistanceSqr =
        hideDistance * hideDistance;


    foreach (Renderer renderer in targetRenderers)
    {
      if (renderer == null)
      {
        continue;
      }


      // --------------------------------------------------------
      // VERIFICA LAYER DO RENDERER
      // --------------------------------------------------------

      if (!IsLayerAllowed(
          renderer.gameObject.layer))
      {
        continue;
      }


      // --------------------------------------------------------
      // POSIÇÃO DO RENDERER
      // --------------------------------------------------------

      Vector3 rendererPosition =
          renderer.transform.position;


      float sqrDistance =
          (
              playerPosition -
              rendererPosition
          ).sqrMagnitude;


      // --------------------------------------------------------
      // ESCONDER
      // --------------------------------------------------------

      if (sqrDistance >= hideDistanceSqr)
      {
        if (!renderer.forceRenderingOff)
        {
          renderer.forceRenderingOff = true;


          if (debugMode)
          {
            Debug.Log(
                $"[DrawDistance] " +
                $"{renderer.name} ESCONDIDO | " +
                $"Distância: " +
                $"{Mathf.Sqrt(sqrDistance):F1}m"
            );
          }
        }
      }
    }
  }


  // ============================================================
  // ALTERAR PLAYER
  // ============================================================

  public void SetPlayer(Player newPlayer)
  {
    playerTarget = newPlayer;
  }


  // ============================================================
  // REFRESH
  // ============================================================

  public void Refresh()
  {
    FindRenderers();


    if (playerTarget == null)
    {
      FindPlayer();
    }


    if (playerTarget != null)
    {
      UpdateDrawDistance();
    }
  }
}
