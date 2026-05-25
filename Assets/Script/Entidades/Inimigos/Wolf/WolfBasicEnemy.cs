using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class WolfBasicEnemy : NavBasedEnemy
{
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

  [Header("Distâncias de Combate")]
  [SerializeField] private float _attackDistance = 10f;
  [SerializeField] private float _minAttackDistance = 2f;

  // Estados possíveis do Lobo: patrulhando ou perseguindo

  [SerializeField]
  private Animator _animimator;

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

  [Header("Patrulha Idle")]
  [SerializeField] private float _minIdleTime = 1.5f;
  [SerializeField] private float _maxIdleTime = 4f;

  private bool _isWaiting = false;
  private bool _returningToPost = false;

  [Header("Sistema de Patrulha")]
  [SerializeField] private int _minPatrolsBeforeIdle = 5;
  [SerializeField] private int _maxPatrolsBeforeIdle = 9;

  private int _currentPatrolCount = 0;
  private int _patrolsBeforeIdle;

  private bool _isIndleAnimation = false;

  protected new void Awake()
  {
    base.Awake();
    _agent = GetComponent<NavMeshAgent>(); // Pega o NavMeshAgent do Lobo
    _vision = GetComponentInChildren<EyeWolf>(); // Procura o Script EyeWolf em filhos (ex: "cabeça/olhos)

    _animimator = GetComponentInChildren<Animator>();

    _startPosition = transform.position; // Salva a posição inicial do inimigo
  }

  protected new void Start()
  {
    _player = GameObject.FindGameObjectWithTag("Player")?.transform; // Localiza o Player pela tag
    _patrolsBeforeIdle =
      Random.Range(_minPatrolsBeforeIdle,
      _maxPatrolsBeforeIdle + 1);

    _animimator.SetBool("isWalking", true);
    _animimator.SetBool("isIdle", false);

    Patrol();  // Inicia patrulhando
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

          _isWaiting = false;
          _agent.isStopped = false;
          _animimator.SetBool("isIdle", false);
          _animimator.SetBool("isWalking", true);
          _memoryTimer = _chaseMemoryTime; // Reseta a memoria de perseguição
        }
        if (_returningToPost)
        {
          if(!_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
          {
            _returningToPost = false;

            StartCoroutine(WaitOnPatrol());
          }

          return;
        }
        else if(!_isWaiting &&
          !_isAttacking &&
          !_agent.pathPending &&
          _agent.remainingDistance < _agent.stoppingDistance)
        {
          _currentPatrolCount++;

          if(_currentPatrolCount < _patrolsBeforeIdle)
          {
            Patrol();
          }

          else
          {
            StartCoroutine(WaitOnPatrol());
          }
        }
        break;

      case WolfState.Chase:
        if (_vision._encontrouPlayer && _vision._playerDetectado != null)
        {
          _memoryTimer = _chaseMemoryTime; // Enquanto vê, reseta o timer
          Chase(_vision._playerDetectado); // Continua perseguindo

          float dis = Vector3.Distance(transform.position, _vision._playerDetectado.position);
          if (!_isAttacking && _dashTimer <= 0f && dis <= _attackDistance)
          {
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
            
            _returningToPost = true;

            _agent.isStopped = false;

            _animimator.SetBool("isWalking", true);
            _animimator.SetBool("isIdle", false);

            _agent.SetDestination(_startPosition);
          }
        }
        break;
    }
  }

  private void Patrol()
  {
    if (!_agent.isOnNavMesh)
      return;

    _agent.speed = _patrolSpeed;

    Vector3 randomPoint =
      _startPosition + UnityEngine.Random.insideUnitSphere * _patrolRadius;

    randomPoint.y = _startPosition.y;

    if(NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
    {
      _agent.SetDestination(hit.position);

      _animimator.SetBool("isWalking", true);
      _animimator.SetBool("isIdle", false);
    }
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
    _isAttacking = true;
    _animimator.SetBool("isAttacking", true);
    _animimator.SetBool("isWalking", false);
    _animimator.SetBool("isIdle", false);

    _agent.isStopped = true;
    _agent.velocity = Vector3.zero;

    // desliga o NavMesh

    _agent.enabled = false;

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

    Vector3 toPlayer = 
      (playerTransform.position - transform.position);
    
    
    toPlayer.y = 0;
    
    Vector3 disered =
      transform.position
      + toPlayer.normalized * Mathf.Min(_rushDistance, toPlayer.magnitude -  1.2f);

    if(NavMesh.SamplePosition(
      disered,
      out NavMeshHit hit,
      1.0f,
      NavMesh.AllAreas))
    {
      disered = hit.position;
    }

    disered.y = transform.position.y;

    // executa a rush

    _currentTweener?.Kill();
    _currentTweener = transform.DOMove(disered, _rushDuration)
                      .SetEase(_rushEase);

    //float elapsed = 0f;

    yield return _currentTweener.WaitForCompletion();

    yield return new WaitForSeconds(0.05f);

    _agent.enabled = true;


    _agent.isStopped = false;


    _agent.ResetPath();

    _isAttacking = false;

    _animimator.SetBool("isAttacking", false);


    if (_vision != null &&
    _vision._encontrouPlayer &&
    _vision._playerDetectado != null)
    {
      float distance =
          Vector3.Distance(
              transform.position,
              _vision._playerDetectado.position);

      // PLAYER AINDA ESTÁ PERTO
      if (distance <= _minAttackDistance)
      {
        yield return new WaitForSeconds(0.15f);

        _isAttacking = false;

        yield break;
      }

      if(distance <= _attackDistance)
      {
        yield return new WaitForSeconds(0.15f);

        _isAttacking = false;

        yield break;
      }

      // PLAYER AFASTOU UM POUCO
      // VOLTA A PERSEGUIR
      _animimator.SetBool("isWalking", true);
      _animimator.SetBool("isIdle", false);

      _agent.SetDestination(
          _vision._playerDetectado.position);
    }
    else
    {
      // PERDEU PLAYER
      _animimator.SetBool("isWalking", true);
      _animimator.SetBool("isIdle", false);

      _agent.SetDestination(_startPosition);
    }

  }

  private IEnumerator WaitOnPatrol()
  {
    if(_isAttacking || 
      _currentState != WolfState.Patrol)
    {
      yield break;

    }

    _isWaiting = true;
    _isIndleAnimation = true;

    _agent.isStopped = true;

    // TRANSIÇÃO PARA IDLE
    _animimator.SetBool("isWalking", false);
    _animimator.SetBool("isIdle", true);

    AnimatorStateInfo state =
      _animimator.GetCurrentAnimatorStateInfo(0);

    yield return null;

    while (_animimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
    {
      yield return null;
    }



    float waitTime = Random.Range(_minIdleTime,_maxIdleTime);

    yield return new WaitForSeconds(waitTime);

    if(_currentState != WolfState.Patrol)
    {
      _isWaiting = false;
      _isIndleAnimation = false;

      yield break;
    }
    
    _animimator.SetBool("isIdle", false);
    _animimator.SetBool("isWalking", true);

    _agent.isStopped = false;

    _currentPatrolCount = 0;

    _patrolsBeforeIdle = 
      Random.Range(_minPatrolsBeforeIdle,
      _maxPatrolsBeforeIdle + 1);


    Patrol();

    _isWaiting = false;
    _isIndleAnimation = false;
  }
}
