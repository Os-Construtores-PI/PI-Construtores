using UnityEngine;

public class EyeWolf : MonoBehaviour
{
    [Header("Config do Campo de Vis�o")]
    public float _visionRange = 10f; // alcance de vis�o
    public float _visionAngle = 120f; // angulo de vis�o
    [Header("Camadas de Detecção")]
    public LayerMask _targetMask; // layer do player ou entities
    public LayerMask _obstacleMask; // layer de obstáculos

    [Header("Debug")]
    public bool _encontrouPlayer;
    public Transform _playerDetectado;

    private Transform target;

    private void Update()
    {
        ProcurarAlvos();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayersHolder");

        if (playerObj != null)
            target = playerObj.transform;

        else
            Debug.LogWarning("Player não encontrado! Verifique a Tag do Player");
    }

    public void ProcurarAlvos()
    {
        _encontrouPlayer = false;
        _playerDetectado = null;


        Collider[] targetsInArea = Physics.OverlapSphere(transform.position, _visionRange, _targetMask);

        foreach (var col in targetsInArea)
        {
            if (CanSeeTarget(col.transform))
            {
                // Se usa Entities -> confere se é Player
                if (col.TryGetComponent<Player>(out Player _player))
                {
                    _encontrouPlayer = true;
                    _playerDetectado = col.transform;
                    Debug.Log("Wolf encontrou o Pandora!!!");
                    break;
                }

                // Se usa o Layer exclusiva Player -> já basta
                // encontrouPlayer = true;
                // player detectado = target.position;
                // break;
            }
        }
    }

    public bool CanSeeTarget(Transform target)
    {
        Vector3 dirToTarget = ((target.position + Vector3.up * 1.5f) - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, target.position);


        // Angulo
        if (Vector3.Angle(transform.forward, dirToTarget) < _visionAngle / 2)
        {
            // Angulo
            if (!Physics.Raycast(transform.position, dirToTarget, dist, _obstacleMask))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Vector3 angleA = DirecaodoAngulo(-_visionAngle / 2);
        Vector3 angleB = DirecaodoAngulo(_visionAngle / 2);

        if (_encontrouPlayer && _playerDetectado != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _playerDetectado.position);
        }
    }

    private Vector3 DirecaodoAngulo(float anguloemGraus)
    {
        float rad = (anguloemGraus + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }

    // Permite definir o target manualmente (opcional)
    public void SetTarget(Transform t)
    {
        _playerDetectado = t;
        _encontrouPlayer = t != null;
    }
}








