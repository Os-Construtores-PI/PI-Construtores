using UnityEngine;

public class TerrainSector : MonoBehaviour
{
  [Header("Objetos desta área")]
  [SerializeField] private GameObject[] _objects;

  [Header("Ponto de ativação")]
  [SerializeField] private Transform _activationPoint;

  [Header("Distância")]
  [SerializeField] private float _activationDistance = 40f;
  [SerializeField] private float _deactivationDistance = 60f;

  [Header("Estado")]
  [SerializeField] private bool _activeStart = false;

  [Header("Performace")]
  [SerializeField] private float _checkInterval = 0.25f;

  private float _activationDistanceSqr;
  private float _deactivationDistanceSqr;

  private float _nextChecktTime;
  private Transform _player;

  private bool _isActive;

  private void Awake()
  {
    _activationDistanceSqr =
      _activationDistance * _activationDistance;

    _deactivationDistanceSqr =
      _deactivationDistance * _deactivationDistance;

    SetSectorActive(_activeStart);
  }

  private void Start()
  {
    FindPlayer();
  }

  private void Update()
  {
    if(_player == null)
    {
      FindPlayer();
      return;
    }

    if (Time.time < _nextChecktTime)
      return;

    _nextChecktTime =
      Time.time + _checkInterval;

    CheckActivation();
  }

  private void CheckActivation()
  {
    if (_activationPoint == null)
      return;

    Vector3 offset = 
      _player.position - _activationPoint.position;

    offset.y = 0f;

    float distanceSqr =
      offset.sqrMagnitude;

    if (!_isActive)
    {
      if(distanceSqr <= _activationDistanceSqr)
      {
        SetSectorActive(true);

        Debug.Log(
          $"[TerrainSector] {name} -> ATIVADO"
          );
      }
       return;
    }

    if (distanceSqr >= _deactivationDistanceSqr)
    {
      SetSectorActive(false);

      Debug.Log(
          $"[TerrainSector] {name} -> DESATIVADO"
      );
    }


  }

  private void SetSectorActive(bool active)
  {
    if (_isActive == active)
        return;
    _isActive = active;

    if (_objects == null)
      return;

    for (int i = 0; i < _objects.Length; i++)
    {
      if (_objects[i] != null)
      {
        _objects[i].SetActive(active);
      }
    }
  }

  private void FindPlayer()
  {
    GameObject player =
      GameObject.FindGameObjectWithTag("Player");

    if(player != null)
    {
      _player = player.transform;
    }
  }

  private void OnDrawGizmosSelected()
  {
    if (_activationPoint == null)
      return;

    Gizmos.color = Color.green;

    Gizmos.DrawWireSphere(
        _activationPoint.position,
        _activationDistance
    );
  }
}
