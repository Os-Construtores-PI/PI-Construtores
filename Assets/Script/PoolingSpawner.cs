using System.Collections.Generic;
using UnityEngine;

public class PoolingSpawner : MonoBehaviour
{
  [SerializeField]
  private GameObject[] _objects;

  [SerializeField]
  private float _spawnDistance = 30f;

  [SerializeField]
  private float _despawnDistance = 50f;

  [SerializeField]
  private Transform _player;

  [SerializeField]
  private bool _spawnAutomatically = true;

  [SerializeField]
  private float _distanceCheckInterval = 0.25f;

  [SerializeField]
  private LayerMask _groundMask = ~0;

  [SerializeField]
  private float _raycastHeight = 50f;

  [SerializeField]
  private float _raycastDistance = 200f;

  private float _nextDistanceCheck;

  private float _spawnDistanceSqr;
  private float _despawnDistanceSqr;

  private bool _objectsActive;

  private Vector3 _referencePoint;

  private void Awake()
  {
    _spawnDistanceSqr = _spawnDistance * _spawnDistance;
    _despawnDistanceSqr = _despawnDistance * _despawnDistance;

    SetObjectsActive(false);
    ChooseReferencePoint();
  }

  private void Start()
  {
    FindPlayer();
  }

  private void Update()
  {
    if (_player == null)
    {
      FindPlayer();
      return;
    }

    if (Time.time < _nextDistanceCheck)
      return;

    _nextDistanceCheck = Time.time + _distanceCheckInterval;

    CheckDistance();
  }

  private void CheckDistance()
  {
    Vector3 offset = _player.position - _referencePoint;
    offset.y = 0f;

    float distanceSqr = offset.sqrMagnitude;

    if (!_objectsActive)
    {
      if (_spawnAutomatically && distanceSqr <= _spawnDistanceSqr)
      {
        ActivateObjects();
      }

      return;
    }

    if (distanceSqr >= _despawnDistanceSqr)
    {
      DeactivateObjects();
      ChooseReferencePoint();
    }
  }

  private void ChooseReferencePoint()
  {
    Vector3 center = GetObjectsCenter();

    Vector3 origin = center + Vector3.up * _raycastHeight;

    if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundMask))
    {
      _referencePoint = hit.point;
    }
    else
    {
      _referencePoint = center;
    }
  }

  private Vector3 GetObjectsCenter()
  {
    if (_objects == null || _objects.Length == 0)
      return transform.position;

    Vector3 sum = Vector3.zero;
    int count = 0;

    for (int i = 0; i < _objects.Length; i++)
    {
      if (_objects[i] == null)
        continue;

      sum += _objects[i].transform.position;
      count++;
    }

    if (count == 0)
      return transform.position;

    return sum / count;
  }

  private void ActivateObjects()
  {
    if (_objectsActive)
      return;

    ChooseReferencePoint();
    SetObjectsActive(true);

    _objectsActive = true;

    Debug.Log($"[Pooling Spawer] Objetos ativados: {name}");
  }

  private void DeactivateObjects()
  {
    if (!_objectsActive)
      return;

    SetObjectsActive(false);

    _objectsActive = false;

    Debug.Log($"[Pooling Spawer] Objetos desativados: {name}");
  }

  private void SetObjectsActive(bool active)
  {
    if (_objects == null)
      return;

    for (int i = 0; i < _objects.Length; i++)
    {
      if (_objects[i] == null)
        continue;

      _objects[i].SetActive(active);
    }
  }

  private void FindPlayer()
  {
    if (_player != null)
      return;

    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

    if (playerObject != null)
    {
      _player = playerObject.transform;
    }
  }

  public void SpawnPack()
  {
    ActivateObjects();
  }

  public void DespawnPack()
  {
    DeactivateObjects();
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(transform.position, _spawnDistance);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, _despawnDistance);

    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(_referencePoint, 0.5f);
  }
}