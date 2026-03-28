using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class WolfBasicEnemy : Enemies
{
    private NavMeshAgent _agent; // controla o movimento do inimigo via NavMesh

    private Transform _player; // Referência ao Player (Pandora)
    private EyeWolf _vision; // Script responsável pela visão do Lobo

    [Header("Distancia de Parada")]
    public float _stopDistance = 10f; // distancia em que o Lobo para de se mover

    [Header("Configurações")]
    public float _patrolRadius = 5f; // distância máxima que o Lobo anda na patrulha
    public float _chaseSpeed = 4f; // velocidade do lobo ao perseguir o Player
    public float _patrolSpeed = 2f; // Velocidade do lobo quando está patrulhando

    [Header("Memoria da Perseguição")]
    public float _chaseMemoryTime = 3f; // tempo (em segundos) que ele continua perseguindo mesmo sem ver o Player
    private float _memoryTimer = 0f; // Contador interno dessa memória

    // Estados possíveis do Lobo: patrulhando ou perseguindo
    private enum WolfState
    {
        Patrol,
        Chase,
    }

    private WolfState _currentState = WolfState.Patrol;

    private Vector3 _startPosition; // Posição inicial do inimigo, usada como centro da patrulha
    public bool _isAttacking = false;
    private Tweener _currentTweener;

    [Header("Rush(DOTWEEN)")]
    [SerializeField]
    private float _prepTime = 0.6f;

    [SerializeField]
    private float _rushDistance = 4f; // distancia máxima do Rush

    [SerializeField]
    private float _rushDuration = 0.35f; // duração do rush

    [SerializeField]
    private Ease _rushEase = Ease.OutQuad;

    [SerializeField]
    private float _hitRadiusDuringRush = 1.2f; // raio de acerto

    [SerializeField]
    private float _attackDamage = 20f;

    [SerializeField]
    private float _dashCooldown = 1.2f; // tempo minimo entre ataques
    private float _dashTimer = 0f;

    protected new void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>(); // Pega o NavMeshAgent do Lobo
        _vision = GetComponentInChildren<EyeWolf>(); // Procura o Script EyeWolf em filhos (ex: "cabeça/olhos)
        _startPosition = transform.position; // Salva a posição inicial do inimigo
    }

    protected new void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform; // Localiza o Player pela tag
        Patrol(); // Inicia patrulhando
    }

    // Update is called once per frame
    protected new void Update()
    {
        if (_dashTimer > 0f)
            _dashTimer -= Time.deltaTime;

        switch (_currentState)
        {
            case WolfState.Patrol:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    _currentState = WolfState.Chase; // Muda para perseguição
                    _memoryTimer = _chaseMemoryTime; // Reseta a memoria de perseguição
                }
                else if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
                {
                    Patrol(); // Se não tem destina ou já chegou, escolhe novo ponto de patrulha
                }
                break;

            case WolfState.Chase:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    _memoryTimer = _chaseMemoryTime; // Enquanto vê, reseta o timer
                    Chase(_vision._playerDetectado); // Continua perseguindo

                    float dis = Vector3.Distance(
                        transform.position,
                        _vision._playerDetectado.position
                    );
                    if (!_isAttacking && _dashTimer <= 0f && dis <= _stopDistance)
                    {
                        _isAttacking = true;
                        _dashTimer = _dashCooldown; // reseta cooldown
                        StartCoroutine(PrepareThenRush(_vision._playerDetectado));
                    }
                }
                else
                {
                    // Se perdeu o player, usa a memória
                    if (_memoryTimer > 0)
                    {
                        _memoryTimer -= Time.deltaTime; // Diminui o contador
                        if (_vision != null && _vision._playerDetectado != null) // Continua indo até a ultima posição conhecida
                        {
                            Chase(_vision._playerDetectado);
                        }
                    }
                    else
                    {
                        // Timer acabou -> volta a patrulhar
                        _currentState = WolfState.Patrol;
                        Patrol();
                    }
                }
                break;
        }
    }

    private void Patrol()
    {
        _agent.speed = _patrolSpeed; // Define velocidade baixa
        Vector3 randomPoint = _startPosition + UnityEngine.Random.insideUnitSphere * _patrolRadius;
        randomPoint.y = _startPosition.y; // Mantém no mesmo nível do chão
        _agent.SetDestination(randomPoint); // Move para o ponto aleatório
    }

    private void Chase(Transform target)
    {
        if (target == null)
            return;
        _agent.speed = _chaseSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(target.position);
    }

    private IEnumerator PrepareThenRush(Transform playerTransform)
    {
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        // gira suavemente para o Player

        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.DORotateQuaternion(targetRot, 0.25f);
        }

        // esperar carregar o ataque

        yield return new WaitForSeconds(_prepTime);

        // calcula destino do Rush

        Vector3 toPlayer = (playerTransform.position - transform.position);
        toPlayer.y = 0;
        Vector3 disered =
            transform.position
            + toPlayer.normalized * Mathf.Min(_rushDistance, toPlayer.magnitude + 0.5f);

        if (NavMesh.SamplePosition(disered, out NavMeshHit _hit, 1.0f, NavMesh.AllAreas))
            disered = _hit.position;

        disered.y = transform.position.y;

        // executa a rush

        _currentTweener?.Kill();
        _currentTweener = transform.DOMove(disered, _rushDuration).SetEase(_rushEase);

        float elapsed = 0f;

        while (elapsed < _rushDuration)
        {
            if (playerTransform == null)
                break;

            float d = Vector3.Distance(transform.position, playerTransform.position);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_currentTweener != null && _currentTweener.IsActive())
            _currentTweener.Kill(true);

        yield return new WaitForSeconds(0.05f);

        _agent.isStopped = false;

        if (_vision != null && _vision._encontrouPlayer && _vision._playerDetectado != null)
            _agent.SetDestination(_vision._playerDetectado.position);
        else
            _agent.SetDestination(_startPosition);

        _isAttacking = false;
    }
}
