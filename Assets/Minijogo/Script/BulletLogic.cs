using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public int damage = 1;
    private int counter = 0;
    private int max = 5;
    private bool can_damage = true;
    private Rigidbody rb;
    public LayerMask targetLayer;
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.down, ForceMode.Impulse);
        targetLayer = LayerMask.GetMask("Player");
        InvokeRepeating(nameof(Counting), 0, 1f);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Damage(other);
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
