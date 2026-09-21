using System.Collections.Generic;
using UnityEngine;

public class PoolingSpawner : MonoBehaviour
{
  [Header("Objetos Spawner")]
  [SerializeField]
  private GameObject[] _wolves;

  [Header("Distancia")]
  [SerializeField]
  private float _spawnDistance = 30f;

  [SerializeField]
  private float _despawnDistance = 50f;

  [Header("Player")]
  [SerializeField]
  private Transform _player;

  [Header("Automatic Spawn")]
  [SerializeField]
  private bool _spawnAutomatically = true;

  [Header("Performance")]
  [SerializeField]
  private float _distanceCheckInterval = 0.25f;

  private float _nextDistanceCheck;

  private float _spawnDistanceSqr;
  private float _despawnDistanceSqr;

  private bool _wolvesActive;

  private void Awake()
  {
    _spawnDistanceSqr = _spawnDistance * _spawnDistance;

    _despawnDistanceSqr = _despawnDistance * _despawnDistance;

    // IMPORTANTE:
    // Começa com todos os lobos desligados.
    SetWolvesActive(false);
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

    // Verifica distância apenas algumas vezes por segundo.
    if (Time.time < _nextDistanceCheck)
      return;

    _nextDistanceCheck = Time.time + _distanceCheckInterval;

    CheckDistance();
  }

  private void CheckDistance()
  {
    Vector3 offset = _player.position - transform.position;

    // Ignora diferença de altura.
    offset.y = 0f;

    float distanceSqr = offset.sqrMagnitude;

    // ==========================================
    // LOBOS DESATIVADOS
    // ==========================================

    if (!_wolvesActive)
    {
      if (_spawnAutomatically && distanceSqr <= _spawnDistanceSqr)
      {
        ActivateWolves();
      }

      return;
    }

    // ==========================================
    // LOBOS ATIVOS
    // ==========================================

    if (distanceSqr >= _despawnDistanceSqr)
    {
      DeactivateWolves();
    }
  }

  private void ActivateWolves()
  {
    if (_wolvesActive)
      return;

    SetWolvesActive(true);

    _wolvesActive = true;

    Debug.Log($"[EyeWolfSpawner] Lobos ativados: {name}");
  }

  private void DeactivateWolves()
  {
    if (!_wolvesActive)
      return;

    SetWolvesActive(false);

    _wolvesActive = false;

    Debug.Log($"[EyeWolfSpawner] Lobos desativados: {name}");
  }

  private void SetWolvesActive(bool active)
  {
    if (_wolves == null)
      return;

    for (int i = 0; i < _wolves.Length; i++)
    {
      GameObject wolf = _wolves[i];

      if (wolf == null)
        continue;

      wolf.SetActive(active);
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
    ActivateWolves();
  }

  public void DespawnPack()
  {
    DeactivateWolves();
  }

  private void OnDrawGizmosSelected()
  {
    // Spawn
    Gizmos.color = Color.green;

    Gizmos.DrawWireSphere(transform.position, _spawnDistance);

    // Despawn
    Gizmos.color = Color.red;

    Gizmos.DrawWireSphere(transform.position, _despawnDistance);
  }
}
