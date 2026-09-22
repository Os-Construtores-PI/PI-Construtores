using UnityEngine;

public class TerrainSector : MonoBehaviour
{
  [Header("Objetos desta área")]
  [SerializeField] private GameObject[] _objects;


  [Header("Pontos de ativação")]
  [SerializeField] private Transform[] _activationPoint;


  [Header("Distância do ponto")]
  [SerializeField] private float _activationDistance = 40f;


  [Header("Estado")]
  [SerializeField] private bool _activeStart = false;


  [Header("Performance")]
  [SerializeField] private float _checkInterval = 0.25f;


  private float _activationDistanceSqr;

  private float _nextCheckTime;

  private Transform _player;

  private bool _isActive;

  // Indica se o jogador já está dentro de algum ponto.
  private bool _playerInsidePoint;


  // ============================================================
  // AWAKE
  // ============================================================

  private void Awake()
  {
    _activationDistanceSqr =
        _activationDistance *
        _activationDistance;

    SetSectorActive(true);
  }


  // ============================================================
  // START
  // ============================================================

  private void Start()
  {
    FindPlayer();
  }


  // ============================================================
  // UPDATE
  // ============================================================

  private void Update()
  {
    if (_player == null)
    {
      FindPlayer();
      return;
    }


    if (Time.time < _nextCheckTime)
      return;


    _nextCheckTime =
        Time.time + _checkInterval;


    CheckActivationPoint();
  }


  // ============================================================
  // VERIFICA PONTOS
  // ============================================================

  private void CheckActivationPoint()
  {
    if (_activationPoint == null ||
        _activationPoint.Length == 0)
    {
      return;
    }


    bool insideAnyPoint = false;


    // ========================================================
    // PROCURA SE O JOGADOR ESTÁ DENTRO DE ALGUM PONTO
    // ========================================================

    for (int i = 0;
         i < _activationPoint.Length;
         i++)
    {
      Transform point =
          _activationPoint[i];


      if (point == null)
        continue;


      Vector3 offset =
          _player.position -
          point.position;


      // Ignora diferença de altura.
      offset.y = 0f;


      float distanceSqr =
          offset.sqrMagnitude;


      if (distanceSqr <=
          _activationDistanceSqr)
      {
        insideAnyPoint = true;
        break;
      }
    }


    // ========================================================
    // ENTROU NO PONTO
    // ========================================================

    if (insideAnyPoint && !_playerInsidePoint)
    {
      ToggleSector();

      Debug.Log(
          $"[TerrainSector] {name} -> " +
          $"JOGADOR ENTROU NO PONTO | " +
          $"Novo estado: {(_isActive ? "ATIVO" : "DESATIVADO")}"
      );
    }


    // ========================================================
    // ATUALIZA ESTADO DO JOGADOR
    // ========================================================

    _playerInsidePoint =
        insideAnyPoint;
  }


  // ============================================================
  // TOGGLE DO TERRENO
  // ============================================================

  private void ToggleSector()
  {
    SetSectorActive(!_isActive);
  }


  // ============================================================
  // ATIVA / DESATIVA
  // ============================================================

  private void SetSectorActive(bool active)
  {
    if (_isActive == active)
      return;


    _isActive = active;


    if (_objects == null)
      return;


    for (int i = 0;
         i < _objects.Length;
         i++)
    {
      if (_objects[i] == null)
        continue;


      _objects[i].SetActive(active);
    }
  }


  // ============================================================
  // PLAYER
  // ============================================================

  private void FindPlayer()
  {
    if (_player != null)
      return;


    GameObject player =
        GameObject.FindGameObjectWithTag("Player");


    if (player != null)
    {
      _player =
          player.transform;
    }
  }


  // ============================================================
  // RESET
  // ============================================================

  public void ResetSector()
  {
    Debug.Log(
        $"[TerrainSector] {name} -> " +
        "RESETANDO PARA O ESTADO INICIAL"
    );


    _nextCheckTime =
        Time.time;


    // Restaura o estado original.
    SetSectorActive(_activeStart);


    // Muito importante:
    // considera que o jogador ainda não entrou
    // no ponto depois do reset.
    _playerInsidePoint = false;
  }


  // ============================================================
  // FORÇA ATIVO
  // ============================================================

  public void ForceActive()
  {
    SetSectorActive(true);


    _nextCheckTime =
        Time.time;


    _playerInsidePoint = false;


    Debug.Log(
        $"[TerrainSector] {name} -> " +
        "ATIVADO FORÇADAMENTE"
    );
  }


  // ============================================================
  // GIZMOS
  // ============================================================

  private void OnDrawGizmosSelected()
  {
    if (_activationPoint == null)
      return;


    for (int i = 0;
         i < _activationPoint.Length;
         i++)
    {
      Transform point =
          _activationPoint[i];


      if (point == null)
        continue;


      Gizmos.color = Color.yellow;


      Gizmos.DrawWireSphere(
          point.position,
          _activationDistance
      );
    }
  }
}
