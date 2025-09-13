using UnityEngine;

public class EyeWolf : MonoBehaviour
{
    [Header("Config do Campo de Visão")]
    public float _visionRange = 10f; // alcance de visão
    public float _visionAngle = 90f; // angulo de visão
    public LayerMask _targetMask;
    public LayerMask _obstacleMask;
    

    public bool CanSeeTarget(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        if (Vector3.Angle(transform.forward, dirToTarget) < _visionAngle / 2f)
        {
            float disToTarget = Vector3.Distance (transform.position, target.position);

            if (!Physics.Raycast(transform.position, dirToTarget, disToTarget, _obstacleMask))
            {
                return disToTarget <= _visionRange;
            }
        }

        return false;
    }
    
    
}
