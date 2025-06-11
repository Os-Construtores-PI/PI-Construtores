
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MiniGroundScript : MonoBehaviour
{
    public int ground_id;
    private Collider Grass;
    private Collider Dirt;
    private float passCD = .4f;
    void Start()
    {
        if (transform.childCount > 2 )
        {
            Grass = transform.GetChild(0).GetComponent<MeshCollider>();
            Dirt = transform.GetChild(1).GetComponent<MeshCollider>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == transform) return;
        if (!other.CompareTag("Player")) return;

        float playerFeetY = other.bounds.min.y;
        float platformTopY = Grass.bounds.max.y;
        MiniPlayerControl MPC = other.GetComponent<MiniPlayerControl>();
        if (playerFeetY < platformTopY && (MPC.lastID == ground_id || MPC.lastID == -1))
        {
            StartCoroutine(PassLogic(other));
        }
    }

    private IEnumerator PassLogic(Collider collider)
    {
        Physics.IgnoreCollision(collider, Grass, true);
        Physics.IgnoreCollision(collider, Dirt, true);
        yield return new WaitForSeconds(passCD);
        Physics.IgnoreCollision(collider, Grass, false);
        Physics.IgnoreCollision(collider, Dirt, false);
    }
}

