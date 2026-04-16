using UnityEngine;

// Componente que aplica dano a qualquer CombatEntities que entrar na área
public class HitboxComponent : MonoBehaviour
{
  [Header("Parâmetros de Dano")]
  [SerializeField]
  private float _maxDamage = 10f; // Dano inicial
  private float damage;

  // Propriedade pública de acesso ao dano
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
