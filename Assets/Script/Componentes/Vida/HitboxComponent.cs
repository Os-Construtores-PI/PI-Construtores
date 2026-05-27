using UnityEngine;
using static Constants.PlayerShakes;

public class HitboxComponent : MonoBehaviour
{
  [Header("Parâmetros de Dano")]
  [SerializeField]
  private float _maxDamage = 10f;
  private float damage;

  [HideInInspector]
  public float Damage
  {
    get => damage;
    set => damage = value;
  }

  private void Start()
  {
    Damage = _maxDamage;
  }
}
