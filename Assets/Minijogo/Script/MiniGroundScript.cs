
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MiniGroundScript : MonoBehaviour
{

    private Collider Grass;
    private Collider Dirt;
    private float passCD = .4f;
    private int points = 1;
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
        if (playerFeetY < platformTopY)
        {
            StartCoroutine(PassLogic(other));
            other.gameObject.GetComponent<MiniPlayerEvents>().AddPontuation(points);
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

