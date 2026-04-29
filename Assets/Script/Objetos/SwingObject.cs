using DG.Tweening;
using UnityEngine;

public class SwingObject : InteractableObject
{
  [Header("Setup")]
  [SerializeField]
  private Transform _pivot;

  [SerializeField]
  private Transform _playerHolder;

  [Header("Swing Settings")]
  [SerializeField]
  private float _maxAngle = 60f;

  [SerializeField]
  private float _duration = 1.2f;

  [SerializeField]
  private float _angularVelocity = 30f;

  [SerializeField]
  private Ease _ease = Ease.InOutSine;

  private bool _forward = true;

  public override void Interaction(Player player)
  {
    StartSwing(player);
  }

  void StartSwing(Player player)
  {
    Transform playerTrans = player.transform;

    Sequence seq = DOTween.Sequence().SetUpdate(UpdateType.Fixed);

    seq.AppendCallback(() =>
    {
      playerTrans.SetParent(_playerHolder);
      player.LocomotionLayer.ChangeState(new PlayerLocomotionStateGrounded(), player);
      player.CharacterController.enabled = false;
    });
    Vector3 startDir = (_playerHolder.position - _pivot.position).normalized;
    Vector3 axis = Vector3.right;

    float startAngle = -_maxAngle;
    float endAngle = _maxAngle;

    if (!_forward)
    {
      (endAngle, startAngle) = (startAngle, endAngle);
    }

    float currentAngle = startAngle;

    seq.Append(
      DOTween
        .To(
          () => currentAngle,
          x =>
          {
            currentAngle = x;

            Quaternion rot = Quaternion.AngleAxis(currentAngle, axis);
            Vector3 offset = rot * startDir;

            playerTrans.position =
              _pivot.position + offset * Vector3.Distance(_pivot.position, _playerHolder.position);

            UpdateRotation(playerTrans, axis, offset);
          },
          endAngle,
          _duration
        )
        .SetEase(_ease)
    );

    seq.AppendCallback(() =>
    {
      playerTrans.SetParent(null);
      player.LocomotionLayer.ChangeState(player.LocomotionLayer.PreviousState, player);
      player.CharacterController.enabled = true;
      Vector3 finalDir = (playerTrans.position - _pivot.position).normalized;
      Vector3 releaseTangent = Vector3.Cross(axis, finalDir).normalized;
      Vector3 finalVector = Vector3.Lerp(releaseTangent, playerTrans.forward, .7f);
      if (!_forward)
        releaseTangent *= -1;
      float radius = Mathf.Pow(Vector3.Distance(_pivot.position, _playerHolder.position), 2);
      float speed = (_angularVelocity * Mathf.Deg2Rad) * radius;
      float boostFactor = 1.5f;

      player.MovementVector = finalVector * speed * boostFactor;
      _forward = !_forward;
    });
    seq.Play();
  }

  private void UpdateRotation(Transform player, Vector3 axis, Vector3 dir)
  {
    Vector3 tangent = Vector3.Cross(axis, dir).normalized;

    tangent.y = 0;

    if (tangent != Vector3.zero)
    {
      player.rotation = Quaternion.LookRotation(tangent);
    }
  }
}
