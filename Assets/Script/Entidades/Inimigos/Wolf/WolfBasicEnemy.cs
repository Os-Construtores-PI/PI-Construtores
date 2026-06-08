using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class WolfBasicEnemy : NavBasedEnemy
{
  #region Fields & Properties
  [Header("Referências")]
  [SerializeField]
  private Transform _player;

  [SerializeField]
  private EyeWolf _vision;

  [SerializeField]
  private Animator _animator; // Correção do typo

  [Header("Configurações de Movimento")]
  [SerializeField]
  private float _stopDistance = 10f;

  [SerializeField]
  private float _patrolRadius = 5f;

  [SerializeField]
  private float _chaseSpeed = 4f;

  [SerializeField]
  private float _patrolSpeed = 2f;

  [Header("Memória de Perseguição")]
  [SerializeField]
  private float _chaseMemoryTime = 3f;
  private float _memoryTimer = 0f;

  [Header("Combate")]
  [SerializeField]
  private float _attackDistance = 10f;

  [SerializeField]
  private float _minAttackDistance = 2f;

  // Otimização: evita calcular raiz quadrada toda frame
  private float _attackDistanceSqr => _attackDistance * _attackDistance;
  private float _minAttackDistanceSqr => _minAttackDistance * _minAttackDistance;

  [Header("Rush (DOTween)")]
  [SerializeField]
  private float _prepTime = 0.6f;

  [SerializeField]
  private float _rushDistance = 4f;

  [SerializeField]
  private float _rushDuration = 0.35f;

  [SerializeField]
  private Ease _rushEase = Ease.OutQuad;

  [SerializeField]
  private float _dashCooldown = 1.2f;

  private float _dashTimer = 0f;
  private bool _isAttacking = false;
  private Tweener _currentTweener;
  private Coroutine _attackCoroutine; // Referência para cancelar se necessário

  [Header("Patrulha e Idle")]
  [SerializeField]
  private float _minIdleTime = 1.5f;

  [SerializeField]
  private float _maxIdleTime = 4f;

  [SerializeField]
  private int _minPatrolsBeforeIdle = 5;

  [SerializeField]
  private int _maxPatrolsBeforeIdle = 9;

  private Vector3 _startPosition;
  private WolfState _currentState = WolfState.Patrol;

  private bool _isWaiting = false;
  private bool _returningToPost = false;
  private int _currentPatrolCount = 0;
  private int _patrolsBeforeIdle;
  private Coroutine _idleCoroutine;

  private enum WolfState
  {
    Patrol,
    Chase,
  }
  #endregion

  

  #region Unity Lifecycle
  public override void Awake()
  {
    base.Awake();

    if (_vision == null)
      _vision = GetComponentInChildren<EyeWolf>();
    if (_animator == null)
      _animator = GetComponentInChildren<Animator>();

    _startPosition = transform.position;
  }

  public override void Start()
  {
    base.Start();

    if (_player == null)
      _player = GameObject.FindGameObjectWithTag("Player")?.transform;

    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    SetAnimationState(true, false);
    Patrol();
  }

  public override void Update()
  {
    base.Update(); // Garante que lógica da pai seja executada

    

    if (_player == null)
      _player = GameObject.FindGameObjectWithTag("Player")?.transform; // acha de fato o transform da Pandora

    if (_dashTimer > 0f)
      _dashTimer -= Time.deltaTime;

    // Early exit se não houver player ou se o agente não estiver pronto
    if (_player == null || _agent == null || !_agent.isOnNavMesh)
      return;

    switch (_currentState)
    {
      case WolfState.Patrol:
        UpdatePatrolState();
        break;
      case WolfState.Chase:
        UpdateChaseState();
        break;
    }
  }

  protected void OnDisable()
  {
    Cleanup();
  }

  protected void OnDestroy()
  {
    Cleanup();
  }
  #endregion

  #region State Logic
  private void UpdatePatrolState()
  {
    // Transição para Chase
    if (_vision != null && _vision._encontrouPlayer && _vision._playerDetectado != null)
    {
      SwitchToChase(_vision._playerDetectado);
      return;
    }

    // Lógica de Retorno à Base
    if (_returningToPost)
    {
      if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
      {
        _returningToPost = false;
        StartIdleWait();
      }
      return;
    }

    // Chegada no ponto de patrulha
    if (
      !_isWaiting
      && !_isAttacking
      && !_agent.pathPending
      && _agent.remainingDistance <= _agent.stoppingDistance
    )
    {
      _currentPatrolCount++;

      if (_currentPatrolCount < _patrolsBeforeIdle)
      {
        Patrol();
      }
      else
      {
        StartIdleWait();
      }
    }
  }

  private void UpdateChaseState()
  {

    if (_isAttacking)
      return;
    

    bool seesPlayer =
      _vision != null && _vision._encontrouPlayer && _vision._playerDetectado != null;

    if (seesPlayer)
    {
      _memoryTimer = _chaseMemoryTime; // Reset timer
      Chase(_vision._playerDetectado);

      // Lógica de Ataque (Rush)
      if (!_isAttacking && _dashTimer <= 0f)
      {
        float distSqr = Vector3.SqrMagnitude(
          transform.position - _vision._playerDetectado.position
        );

        if (distSqr <= _attackDistanceSqr)
        {
          _dashTimer = _dashCooldown;

          // Cancela ataque anterior se existir (segurança)
          if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
          _attackCoroutine = StartCoroutine(PrepareThenRush(_vision._playerDetectado));
        }
      }
    }
    else
    {
      // Lógica de Memória (Perdeu a visão mas lembra da última posição)
      if (_memoryTimer > 0f)
      {
        _memoryTimer -= Time.deltaTime;
        if (_vision?._playerDetectado != null)
        {
          Chase(_vision._playerDetectado);
        }
      }
      else
      {
        // Esqueceu -> Volta a Patrulhar
        SwitchToPatrol();
      }
    }
  }
  #endregion

  #region Actions
  private void SwitchToChase(Transform target)
  {
    _currentState = WolfState.Chase;
    _isWaiting = false;
    _returningToPost = false;

    StopIdleCoroutine(); // Interrompe idle se estiver esperando

    _agent.isStopped = false;
    _agent.speed = _chaseSpeed;

    SetAnimationState(true, false);
    _memoryTimer = _chaseMemoryTime;
  }

  private void SwitchToPatrol()
  {
    _currentState = WolfState.Patrol;
    _returningToPost = true;

    _agent.isStopped = false;
    _agent.speed = _patrolSpeed;

    SetAnimationState(true, false);

    // Retorna para a posição inicial
    _agent.SetDestination(_startPosition);
  }

  private void Patrol()
  {
    if (!_agent.isOnNavMesh)
      return;

    _agent.speed = _patrolSpeed;
    _agent.isStopped = false;

    // Gera ponto aleatório dentro do raio
    Vector3 randomPoint = _startPosition + Random.insideUnitSphere * _patrolRadius;
    randomPoint.y = _startPosition.y;

    if (
      NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _patrolRadius * 2f, NavMesh.AllAreas)
    )
    {
      _agent.SetDestination(hit.position);
      SetAnimationState(true, false);
    }
  }

  

  private void Chase(Transform target)
  {
    if(_isAttacking)
      return;

    if (target == null || !_agent.isOnNavMesh)
      return;

    _agent.speed = _chaseSpeed;
    _agent.isStopped = false;
    _agent.SetDestination(target.position);
  }

  private IEnumerator PrepareThenRush(Transform playerTransform)
  {
    _isAttacking = true;

    _animator.SetBool("isWalking", false);
    _animator.SetBool("isIdle", false);

    _animator.SetBool("isAttacking", true);


    _animator.SetTrigger("AttackCombo");


    SetAnimationState(false, false);

    // Prepara para o ataque: para o NavMeshAgent
    _agent.isStopped = true;
    _agent.velocity = Vector3.zero;
    _agent.enabled = false; // Desabilita navegação durante animação

    // Rotação suave para o alvo
    Vector3 dir = playerTransform.position - transform.position;
    dir.y = 0;

    if (dir.sqrMagnitude > 0.001f)
    {
      Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
      transform.DORotateQuaternion(targetRot, 0.25f);
    }

    yield return new WaitForSeconds(_prepTime);

    // Calcula destino do Dash
    Vector3 toPlayer = (playerTransform.position - transform.position);
    toPlayer.y = 0;

    // Garante que não dash através do player (subtrai margem de segurança)
    float rushMagnitude = Mathf.Min(_rushDistance, toPlayer.magnitude - _minAttackDistance);
    Vector3 desiredPos = transform.position + toPlayer.normalized * rushMagnitude;

    if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
    {
      desiredPos = hit.position;
    }
    desiredPos.y = transform.position.y;

    // Executa o Dash com DOTween
    _currentTweener?.Kill();
    _currentTweener = transform.DOMove(desiredPos, _rushDuration).SetEase(_rushEase);

    yield return _currentTweener.WaitForCompletion();
    yield return new WaitForSeconds(0.05f); // Pequeno delay pós-impacto

    // Reabilita NavMesh
    _agent.enabled = true;
    _agent.isStopped = false;
    _agent.ResetPath();

    // Reavalia situação pós-ataque
    yield return EvaluatePostAttack(playerTransform);

    _isAttacking = false;
    _animator.SetInteger(
      "AttackIndex",
      -1);
    _attackCoroutine = null;

    _isAttacking = false;

    _animator.SetBool("isAttacking", false);
  }

  private IEnumerator EvaluatePostAttack(Transform playerTransform)
  {
    // Verifica se o player ainda é um alvo válido
    bool seesPlayer =
      _vision != null && _vision._encontrouPlayer && _vision._playerDetectado != null;

    if (seesPlayer)
    {
      float distSqr = Vector3.SqrMagnitude(transform.position - playerTransform.position);

      // Se o player ainda está muito perto, não faz nada (combo ou recuo)
      if (distSqr <= _minAttackDistanceSqr)
      {
        yield return new WaitForSeconds(0.15f);
        yield break;
      }

      // Se está na distância de ataque, pode preparar outro
      if (distSqr <= _attackDistanceSqr)
      {
        yield return new WaitForSeconds(0.15f);
        yield break;
      }

      // Player se afastou um pouco, mas ainda visível: volta a perseguir
      SetAnimationState(true, false);
      Chase(playerTransform);
    }
    else
    {
      // Perdeu o player: volta para base
      SetAnimationState(true, false);
      SwitchToPatrol();
    }
  }

  private void StartIdleWait()
  {
    if (_idleCoroutine != null)
      StopCoroutine(_idleCoroutine);
    _idleCoroutine = StartCoroutine(WaitOnPatrol());
  }

  private IEnumerator WaitOnPatrol()
  {
    if (_isAttacking || _currentState != WolfState.Patrol)
      yield break;

    _isWaiting = true;
    _agent.isStopped = true;
    SetAnimationState(false, true);

    // Aguarda transição de animação
    yield return null;
    while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
    {
      yield return null;
    }

    // Tempo de espera aleatório
    float waitTime = Random.Range(_minIdleTime, _maxIdleTime);
    yield return new WaitForSeconds(waitTime);

    // Verifica se o estado não mudou durante a espera
    if (_currentState != WolfState.Patrol)
      yield break;

    SetAnimationState(true, false);

    _agent.isStopped = false;
    _currentPatrolCount = 0;
    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    Patrol();
    _isWaiting = false;
    _idleCoroutine = null;
  }
  #endregion

  #region Helpers & Cleanup
  private void SetAnimationState(bool walking, bool idle)
  {
    if(_animator == null)
       return;

    _animator.SetBool("isWalking", walking);
    _animator.SetBool("isIdle", idle);
  }

  private void StopIdleCoroutine()
  {
    if (_idleCoroutine != null)
    {
      StopCoroutine(_idleCoroutine);
      _idleCoroutine = null;
      _isWaiting = false;
    }
  }

  private void Cleanup()
  {
    _currentTweener?.Kill();
    _currentTweener = null;

    if (_attackCoroutine != null)
      StopCoroutine(_attackCoroutine);
    StopIdleCoroutine();
  }
  #endregion
}
