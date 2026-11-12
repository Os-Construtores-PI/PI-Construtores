using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ParedeExplosiva : MonoBehaviour, IRespawnable
{
  [Serializable]
  private class Block
  {
    public Block(Rigidbody rb, GameObject go, Vector3 initPos)
    {
      Rb = rb;
      Go = go;
      InitPos = initPos;
    }

    public Rigidbody Rb;
    public Vector3 InitPos;
    public GameObject Go;
  }

  [Header("Explosion Settings")]
  [SerializeField]
  private float _explosionForce = 300f;

  [SerializeField]
  private float _explosionRadius = 5f;

  [SerializeField]
  private float _upsideForce = 1f;

  [SerializeField]
  private float _activeInterval = 2f;

  private List<Block> _blocks = new();
  private bool _destroyed = false;
  public bool IsAlive => gameObject.activeInHierarchy;

  private void Start()
  {
    Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
    foreach (Rigidbody rb in rbs)
    {
      _blocks.Add(new(rb, rb.gameObject, rb.transform.position));
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player") && !_destroyed)
    {
      _destroyed = true;
      WallExplode(other.transform.position);
    }
  }

  void WallExplode(Vector3 posicaoDoToque)
  {
    foreach (Block block in _blocks)
    {
      block.Rb.isKinematic = false;
      block.Rb.AddExplosionForce(_explosionForce, posicaoDoToque, _explosionRadius, _upsideForce);
      block.Rb.mass = 0.5f;
      DOVirtual.DelayedCall(_activeInterval, () => gameObject.SetActive(false));
    }
  }

  public void Respawn()
  {
    foreach (Block block in _blocks)
    {
      block.Rb.isKinematic = true;
      block.Rb.mass = 0.5f;
      gameObject.SetActive(true);
    }
  }
}
