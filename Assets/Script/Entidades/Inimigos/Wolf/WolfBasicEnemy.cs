using System.Collections;
using DG.Tweening;
using UnityEngine;

public class WolfBasicEnemy : NavBasedEnemy
{
  #region Serialized Fields

  [Header("References")]
  [SerializeField]
  private EyeWolf _vision;

  [SerializeField]
  private Animator _animator;

  [Header("Movement Settings")]
  [SerializeField]
  private float _stopDistance = 10f;

  [SerializeField]
  private float _patrolRadius = 5f;

  [SerializeField]
  private float _chaseSpeed = 4f;

  [SerializeField]
  private float _patrolSpeed = 2f;

  [Header("Chase Memory")]
  [SerializeField]
  private float _chaseMemoryTime = 3f;

  [Header("Combat")]
  [SerializeField]
  private float _attackDistance = 10f;

  [SerializeField]
  private float _minAttackDistance = 2f;

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

  [Header("Patrol & Idle")]
  [SerializeField]
  private float _minIdleTime = 1.5f;

  [SerializeField]
  private float _maxIdleTime = 4f;

  [SerializeField]
  private int _minPatrolsBeforeIdle = 5;

  [SerializeField]
  private int _maxPatrolsBeforeIdle = 9;

  [SerializeField] Rigidbody _rb;
  private Vector3 _patrolTarget;

  #endregion

  #region Private Fields

  private float _memoryTimer;
  private float _dashTimer;
  private bool _isAttacking;
  private Tweener _currentTweener;
  private Coroutine _attackCoroutine;
  private Coroutine _idleCoroutine;
  private Transform _playerTransform;

  private Transform _currentTarget;

  private Vector3 _startPosition;
  private WolfState _currentState = WolfState.Patrol;

  private bool _isWaiting;
  private bool _returningToPost;
  private int _currentPatrolCount;
  private int _patrolsBeforeIdle;

  private float AttackDistanceSqr => _attackDistance * _attackDistance;
  private float MinAttackDistanceSqr => _minAttackDistance * _minAttackDistance;

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

    _vision ??= GetComponentInChildren<EyeWolf>();
    _animator ??= GetComponentInChildren<Animator>();

    _rb ??= GetComponent<Rigidbody>();

    _startPosition = transform.position;
  }

  public override void Start()
  {
    base.Start();

    _playerTransform ??= GameObject.FindGameObjectWithTag("Player")?.transform;

    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    SetAnimationState(isWalking: true, isIdle: false);
    Patrol();
  }

  public override void Update()
  {
    base.Update();

    if (_playerTransform == null)
      _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

    if (_dashTimer > 0f)
      _dashTimer -= Time.deltaTime;


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

  private void FixedUpdate()
  {
    if (_isAttacking)
      return;

    if (_isWaiting)
      return;

    switch (_currentState)
    {
      case WolfState.Patrol:
        MoveToPatrolPoint();
        break;

      case WolfState.Chase:
        MoveToTarget();
        break;
    }
  }

  protected void OnDisable() => Cleanup();

  protected void OnDestroy() => Cleanup();

  #endregion

  #region State Updates

  private void UpdatePatrolState()
  {
    if(_vision !=  null && _vision.DetectedPlayer != null)
    {
      StopIdleCoroutine();

      SetAnimationState(false, false);

      SwitchToChase(_vision.DetectedPlayer);
      return;
    } 

    if (_vision != null && _vision.DetectedPlayer != null)
    {
      SwitchToChase(_vision.DetectedPlayer);
      return;
    }

    
  }

  private void UpdateChaseState()
  {
    bool seesPlayer = _vision != null && _vision.DetectedPlayer != null;

    if (seesPlayer)
    {
      _memoryTimer = _chaseMemoryTime;
      Chase(_vision.DetectedPlayer);

      if (!_isAttacking && _dashTimer <= 0f)
      {
        float distSqr = Vector3.SqrMagnitude(transform.position - _vision.DetectedPlayer.position);

        if (distSqr <= AttackDistanceSqr)
        {
          _dashTimer = _dashCooldown;
          if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
          _attackCoroutine = StartCoroutine(PrepareThenRush(_vision.DetectedPlayer));
        }
      }
    }
    else
    {
      if (_memoryTimer > 0f)
      {
        _memoryTimer -= Time.deltaTime;
        if (_vision?.DetectedPlayer != null)
        {
          Chase(_vision.DetectedPlayer);
        }
      }
      else
      {
        SwitchToPatrol();
      }
    }
  }

  private void MoveToPatrolPoint()
  {
    float distance =
      Vector3.Distance(
        transform.position,
        _patrolTarget);

    if( distance < 1f)
    {
      _currentPatrolCount++;

      if(_currentPatrolCount >= _patrolsBeforeIdle)
      {
        StartIdleWait();
        return;
      }

      Patrol();
      return;
    }

    Vector3 dir = 
      (_patrolTarget - transform.position).normalized;

    dir.y = 0;

    _rb.MovePosition(
      transform.position +
      dir * _patrolSpeed * Time.fixedDeltaTime);

    RotateTowards(_patrolTarget);

  }

  #endregion

  #region State Transitions

  private void SwitchToChase(Transform target)
  {
    _currentState = WolfState.Chase;
    _isWaiting = false;
    _returningToPost = false;

    StopIdleCoroutine();

    

    SetAnimationState(isWalking: true, isIdle: false);
    _memoryTimer = _chaseMemoryTime;
  }

  private void SwitchToPatrol()
  {
    _currentState = WolfState.Patrol;
    _returningToPost = true;

    

    SetAnimationState(isWalking: true, isIdle: false);
    
  }

  #endregion

  #region Movement Actions

  private void Patrol()
  {
    Vector3 randomPoint =
      _startPosition +
      Random.insideUnitSphere * _patrolRadius;

    randomPoint.y = transform.position.y;

    _patrolTarget = randomPoint;
  }

  private void Chase(Transform target)
  {
    _currentTarget = target;
  }


  private void MoveToTarget()
  {
    if (_currentTarget == null)
      return;

    Vector3 dir =
      (_currentTarget.position - transform.position).normalized;

    dir.y = 0;

    _rb.MovePosition(
      transform.position +
      dir * _chaseSpeed * Time.fixedDeltaTime);

    RotateTowards(_currentTarget.position);
  }

  private void RotateTowards(Vector3 targetPos)
  {
    Vector3 dir = targetPos - transform.position;
    dir.y = 0;

    if (dir.sqrMagnitude < 0.01f)
      return;

    Quaternion targetRot =
      Quaternion.LookRotation(dir);

    transform.rotation =
      Quaternion.Slerp(
        transform.rotation,
        targetRot,
        8f * Time.deltaTime);
  }
  #endregion

  #region Attack & Animation

  private IEnumerator PrepareThenRush(Transform playerTransform)
  {
    StopIdleCoroutine();
    _isWaiting = false;

    _isAttacking = true;

    _animator.SetBool("isAttacking", true);
    _animator.SetTrigger("AttackCombo");
    SetAnimationState(isWalking: false, isIdle: false);


    Vector3 dir = playerTransform.position - transform.position;
    dir.y = 0f;
    if (dir.sqrMagnitude > 0.001f)
    {
      Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
      transform.DORotateQuaternion(targetRot, 0.25f);
    }

    yield return new WaitForSeconds(_prepTime);

    Vector3 toPlayer = playerTransform.position - transform.position;
    toPlayer.y = 0f;

    float rushMagnitude = Mathf.Min(_rushDistance, toPlayer.magnitude - _minAttackDistance);
    Vector3 desiredPos = transform.position + toPlayer.normalized * rushMagnitude;

    

    _currentTweener?.Kill();
    _currentTweener = transform.DOMove(desiredPos, _rushDuration).SetEase(_rushEase);

    yield return _currentTweener.WaitForCompletion();
    yield return new WaitForSeconds(0.05f);

   

    yield return EvaluatePostAttack(playerTransform);

    if(_currentState == WolfState.Chase)
    {
      _animator.SetBool("isWalking", true);
      _animator.SetBool("isIdle", false);
    }
    else
    {
      _animator.SetBool("isWalking", true);
      _animator.SetBool("isIdle", true);
    }
    
    _animator.SetBool("isAttacking", false);

    _isAttacking = false;

    _animator.SetInteger("AttackIndex", -1);
    _attackCoroutine = null;
  }

  private IEnumerator EvaluatePostAttack(Transform playerTransform)
  {
    bool seesPlayer = _vision != null && _vision.DetectedPlayer != null;

    if (seesPlayer)
    {
      float distSqr = Vector3.SqrMagnitude(transform.position - playerTransform.position);

      if (distSqr <= MinAttackDistanceSqr)
      {
        yield return new WaitForSeconds(0.15f);
        yield break;
      }

      if (distSqr <= AttackDistanceSqr)
      {
        yield return new WaitForSeconds(0.15f);
        yield break;
      }

      SetAnimationState(isWalking: true, isIdle: false);
      Chase(playerTransform);
    }
    else
    {
      SetAnimationState(isWalking: true, isIdle: false);
      SwitchToPatrol();
    }
  }

  #endregion

  #region Idle Handling

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
    
    SetAnimationState(isWalking: false, isIdle: true);

    yield return null;

    yield return new WaitForSeconds(0.5f);

    float waitTime = Random.Range(_minIdleTime, _maxIdleTime);
    yield return new WaitForSeconds(waitTime);

    if (_currentState != WolfState.Patrol)
      yield break;

    SetAnimationState(isWalking: true, isIdle: false);

    
    _currentPatrolCount = 0;
    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    Patrol();
    _isWaiting = false;
    _idleCoroutine = null;
  }

  #endregion

  #region Helpers

  private void SetAnimationState(bool isWalking, bool isIdle)
  {
    if (_animator == null)
      return;
    _animator.SetBool("isWalking", isWalking);
    _animator.SetBool("isIdle", isIdle);
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
