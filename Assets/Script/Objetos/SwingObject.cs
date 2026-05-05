using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SwingObject : MonoBehaviour
{
  [Header("Setup")]
  [SerializeField]
  private RopeMeshObject _rope;

  [Header("Swing Settings")]
  [SerializeField]
  private float _baseMaxAngle = 50f;

  [SerializeField]
  private float _baseDuration = 1.0f;

  [SerializeField]
  private float _referenceSpeed = 8f;

  [SerializeField]
  private float _absoluteMaxAngle = 85f;

  [SerializeField]
  private Ease _ease = Ease.InSine; // InSine acelera até o fim, melhor para lançamento

  [Header("Safety Checks")]
  [SerializeField]
  private LayerMask _groundLayer;

  [SerializeField]
  private float _groundClearance = 1.2f;

  [Header("Velocity & Launch")]
  [SerializeField]
  [Range(0f, 1f)]
  private float _horizontalBias = 0.7f;

  [SerializeField]
  [Range(0f, 1f)]
  private float _verticalDampen = 0.5f;

  [SerializeField]
  private float _maxLaunchSpeed = 30f;

  [SerializeField]
  private float _angleBoostMultiplier = 1.5f;

  [SerializeField]
  private float _launchImpulseMultiplier = 1.8f;

  [SerializeField]
  private float _cooldownDuration = 0.5f;

  private readonly Dictionary<Collider, float> _playerCooldowns = new Dictionary<Collider, float>();

  public void OnTriggerEnter(Collider other)
  {
    if (!other.TryGetComponent(out Player player) || player.IsGrounded)
      return;

    if (
      _playerCooldowns.TryGetValue(other, out float nextAllowedTime)
      && Time.time < nextAllowedTime
    )
      return;

    if (Physics.Raycast(player.transform.position, Vector3.down, 0.5f, _groundLayer))
      return;

    if (player.LocomotionLayer.CurrentState != player.LockedS && player.JumpInputPressed)
      StartSwing(player, other);
  }

  private void StartSwing(Player player, Collider playerCollider)
  {
    Transform playerTrans = player.transform;
    Vector3 entryVelocity = player.MovementVector;
    float entrySpeed = new Vector3(entryVelocity.x, 0, entryVelocity.z).magnitude;
    float speedRatio = entrySpeed / Mathf.Max(_referenceSpeed, 0.01f);

    // 1. EIXO E DIREÇÃO
    Vector3 playerToPivotDir = playerTrans.position - transform.position;
    playerToPivotDir.y = 0;
    Vector3 moveDir = entrySpeed > 0.5f ? entryVelocity.normalized : -playerToPivotDir.normalized;
    moveDir.y = 0;

    Vector3 swingAxis = Vector3.Cross(Vector3.up, moveDir).normalized;

    // 2. RAIO ANTI-CLIPPING
    float radius = Vector3.Distance(transform.position, playerTrans.position);
    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, _groundLayer))
    {
      float maxRadius = hit.distance - _groundClearance;
      if (maxRadius < 1f)
        return;
      radius = Mathf.Min(radius, maxRadius);
    }

    // 3. DINÂMICA
    float dynamicMaxAngle = Mathf.Clamp(
      _baseMaxAngle * Mathf.Lerp(0.7f, _angleBoostMultiplier, speedRatio),
      15f,
      _absoluteMaxAngle
    );
    float dynamicDuration = _baseDuration * (1f - 0.2f * Mathf.Clamp01(speedRatio));

    // 4. SEQUÊNCIA
    // Começa atrás (ângulo positivo) e vai para frente (negativo)
    Quaternion startRot = Quaternion.AngleAxis(dynamicMaxAngle, swingAxis);
    Vector3 startPos = transform.position + (startRot * Vector3.down * radius);

    Sequence seq = DOTween.Sequence().SetUpdate(UpdateType.Fixed).SetLink(gameObject);

    seq.AppendCallback(() =>
    {
      player.LocomotionLayer.ChangeState(player.LockedS, player);
      player.CharacterController.enabled = false;
      _rope.SetVisible(true);
      _rope.SetPoints(transform, playerTrans);
    });

    // Snap inicial
    seq.Append(playerTrans.DOMove(startPos, 0.12f).SetEase(Ease.OutQuad));
    seq.Join(playerTrans.DOLookAt(playerTrans.position + moveDir, 0.12f, AxisConstraint.Y));

    float currentAngle = dynamicMaxAngle;
    seq.Append(
      DOTween
        .To(
          () => currentAngle,
          x =>
          {
            currentAngle = x;
            Quaternion rot = Quaternion.AngleAxis(currentAngle, swingAxis);
            Vector3 offset = rot * Vector3.down;
            playerTrans.position = transform.position + (offset * radius);

            Vector3 tangent = Vector3.Cross(swingAxis, offset).normalized;
            if (Vector3.Dot(tangent, moveDir) < 0)
              tangent *= -1f;

            if (tangent.sqrMagnitude > 0.01f)
              playerTrans.rotation = Quaternion.LookRotation(tangent);
          },
          -dynamicMaxAngle,
          dynamicDuration
        )
        .SetEase(_ease)
    );

    seq.OnComplete(() =>
    {
      Launch(player, swingAxis, moveDir, radius, speedRatio, dynamicMaxAngle);
      _playerCooldowns[playerCollider] = Time.time + _cooldownDuration;
    });

    seq.Play();
  }

  private void Launch(
    Player player,
    Vector3 axis,
    Vector3 moveDir,
    float radius,
    float speedRatio,
    float maxAngle
  )
  {
    Transform pt = player.transform;
    player.CharacterController.enabled = true;
    player.LocomotionLayer.ChangeState(player.AirborneS, player);
    _rope.SetVisible(false);

    // --- CÁLCULO DE FORÇA MELHORADO ---
    // Comprimento do arco total percorrido
    float arcDistance = (maxAngle * 2 * Mathf.Deg2Rad) * radius;

    // Velocidade média = d/t. No InSine, a velocidade final é ~2x a média.
    float speed = (arcDistance / _baseDuration) * 2.0f;

    // Aplica multiplicadores de entrada e game feel
    float finalSpeed =
      speed * Mathf.Lerp(1f, _angleBoostMultiplier, speedRatio) * _launchImpulseMultiplier;
    finalSpeed = Mathf.Clamp(finalSpeed, 12f, _maxLaunchSpeed);

    // --- DIREÇÃO DE LANÇAMENTO (FIX PARA 90°) ---
    // Em vez de usar apenas a tangente (que vira vertical em 90°),
    // nós misturamos a direção que o player quer ir com um impulso para cima.

    Vector3 launchForward = moveDir; // Direção horizontal pura
    Vector3 launchUp = Vector3.up; // Direção vertical pura

    // Se o ângulo for muito alto, diminuímos a influência vertical para não virar um foguete
    float verticalForce = Mathf.Cos(maxAngle * Mathf.Deg2Rad) * _verticalDampen;
    // Se maxAngle for 90, verticalForce vira 0. Então forçamos um mínimo para o pulo ser bonito:
    verticalForce = Mathf.Clamp(verticalForce, 0.2f, 0.5f);

    Vector3 finalDir = Vector3
      .Lerp(launchUp * verticalForce, launchForward, _horizontalBias)
      .normalized;

    // Aplica o vetor final
    player.MovementVector = finalDir * finalSpeed;

    // Pequeno bônus: se o player estiver indo muito devagar, damos um "push" mínimo
    if (player.MovementVector.magnitude < 10f)
      player.MovementVector = finalDir * 10f;
  }
}
