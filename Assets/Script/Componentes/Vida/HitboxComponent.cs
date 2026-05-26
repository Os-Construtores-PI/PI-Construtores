using UnityEngine;

// Componente que aplica dano a qualquer CombatEntities que entrar na área
public class HitboxComponent : MonoBehaviour
{
  [Header("Parâmetros de Dano")]
  [SerializeField]
  private float _maxDamage = 10f; // Dano inicial
  private float damage;

  [Header("KnockBack")]
  private float _knockBackForce = 2.5f;

  [SerializeField] private float _upForce = 1f;

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

  private void OTriggerEnter(Collider other)
  {
    if(!other.CompareTag("Player"))
       return;
    
    Rigidbody rb = other.GetComponent<Rigidbody>();

    if(rb != null)
    {
      Vector3 dir = (other.transform.position - transform.position).normalized;

      dir.y = 0f;

      Vector3 force = dir * _knockBackForce + Vector3.up * _upForce;

      rb.AddForce(force, ForceMode.Impulse);
    }
  }
}
