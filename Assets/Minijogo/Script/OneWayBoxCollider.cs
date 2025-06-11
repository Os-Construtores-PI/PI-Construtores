using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class OneWayBoxCollider : MonoBehaviour

   

{
    [SerializeField] private  Vector3 entryDirection = Vector3.up;
    [SerializeField] private bool localDirection = false;
    private new BoxCollider collider = null;

    private BoxCollider collisionCheckTrigger = null;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        collider.isTrigger = false;

        collisionCheckTrigger = GetComponent<BoxCollider>();
        
       
    }
    private void OnCollisionStay(Collision collision)
    {
        
    }

    private void OnDrawGizmosSelected()
    {

        Vector3 direction;
        if (localDirection)
        {
            direction = transform.TransformDirection(entryDirection.normalized);
        }
        else
        {
            direction = entryDirection;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, entryDirection);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -entryDirection);
    }
}
