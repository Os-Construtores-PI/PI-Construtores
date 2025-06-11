using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public int damage = 1;
    private int counter = 0;
    private int max = 5;
    private bool can_damage = true;
    private Rigidbody rb;
    public float coneAngle = 45f;
    public float coneRadius = 5f;
    public LayerMask targetLayer;
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.down, ForceMode.Impulse);
        targetLayer = LayerMask.GetMask("Player");
        InvokeRepeating(nameof(Counting), 0, 1f);
    }
        void Update()
    {
        DetectTargetsInCone();
    }

    void DetectTargetsInCone()
    {
        // Pega todos os objetos próximos em uma esfera
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, coneRadius, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget < coneAngle * 0.5f)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    if (can_damage)
                    {
                        Damage(hitCollider);                                       
                    }
                }
            }
        }
    }
    void Awake()
    {
        damage = 1;
    }
    void Counting()
    {
        counter += 1;
        if (counter == max)
        {
            Destroy(gameObject);
        }
    }
    void Damage(Collider collider)
    {
        MiniPlayerEvents script = collider.GetComponent<MiniPlayerEvents>();
        script.DamagePlayer(damage);
        can_damage = false;
    }
}
