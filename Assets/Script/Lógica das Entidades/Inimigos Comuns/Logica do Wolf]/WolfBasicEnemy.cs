using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class WolfBasicEnemy : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _player;
    private EyeWolf _vision;

    [Header("Configurações")]
    public float _patrolRadius = 5f;
    public float _chaseSpeed = 4f;
    public float _patrolSpeed = 2f;

    private Vector3 _startPosition;

    private enum WolfState  {Patrol, Chase}
    private WolfState _currentState = WolfState.Patrol;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _vision = GetComponentInChildren<EyeWolf>();
        _startPosition = transform.position;
    }
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Patrol();

    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case WolfState.Patrol:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    _currentState = WolfState.Chase;
                }
                else if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
                {
                    Patrol();
                }
                break;
            case WolfState.Chase:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    Chase(_vision._playerDetectado);
                }
                else
                {
                    _currentState = WolfState.Patrol;
                    Patrol();
                }
                break;
        }


              
    }

    



    private void Patrol()
    {
        _agent.speed = _patrolSpeed;
        Vector3 randomPoint = _startPosition + Random.insideUnitSphere * _patrolRadius;
        randomPoint.y = _startPosition.y;
        _agent.SetDestination(randomPoint);
    }

    private void Chase(Transform target)
    {
        _agent.speed = _chaseSpeed;
        _agent.SetDestination(target.position);
    }

}
