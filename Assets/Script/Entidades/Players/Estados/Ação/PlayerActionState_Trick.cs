using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class PlayerActionStateTrick : IPlayerState<Player>
{
  public enum TrickDirection
  {
    None, Left, Up, Right, Down
  }

  [Serializable]
  public class DirectionRotation
  {
    public TrickDirection Direction;
    public Vector3 Rotation = new(0f, 360f, 0f);
    public float Duration = 0.5f;
    public Ease Ease = Ease.OutQuad;
  }

  [Serializable]
  public class TrickSequence
  {
    public string Name = "Combo";
    public List<TrickDirection> Directions = new();
    public int Score = 500;
  }

  public float TrickDeadzone = .6f;
  [SerializeField]
  private int _freeTrickScore = 100;
  [SerializeField]
  private float _sequenceTimeout = 0.35f;
  [SerializeField]
  private float _groundedGraceTime = 0.1f;
  [SerializeField]
  private float _landingResetDuration = 0.1f;
  [SerializeField]
  private Ease _landingResetEase = Ease.OutQuad;
  [SerializeField]
  private List<DirectionRotation> _rotations = new()
  {
    new DirectionRotation { Direction = TrickDirection.Left, Rotation = new Vector3(0f, -360f, 0f) },
    new DirectionRotation { Direction = TrickDirection.Right, Rotation = new Vector3(0f, 360f, 0f) },
    new DirectionRotation { Direction = TrickDirection.Up, Rotation = new Vector3(360f, 0f, 0f) },
    new DirectionRotation { Direction = TrickDirection.Down, Rotation = new Vector3(-360f, 0f, 0f) },
  };
  [SerializeField]
  private List<TrickSequence> _sequences = new();

  public PlayerActionType Type => PlayerActionType.Trick;

  public HashSet<PlayerActionType> IncompatibleActions => new();

  readonly List<TrickDirection> buffer = new();
  Tween trickTween;
  Tween resetTween;
  Quaternion restRotation;
  float bufferTimer;
  float timeInState;

  public void Enter(Player entity)
  {
    if (resetTween != null && resetTween.IsActive())
      resetTween.Complete();

    restRotation = entity.Model.localRotation;
    buffer.Clear();
    bufferTimer = 0f;
    timeInState = 0f;
  }

  public void Exit(Player entity)
  {
    buffer.Clear();

    if (trickTween != null && trickTween.IsActive())
      trickTween.Kill();

    if (_landingResetDuration <= 0f)
    {
      entity.Model.localRotation = restRotation;
      return;
    }

    resetTween = entity.Model
      .DOLocalRotateQuaternion(restRotation, _landingResetDuration)
      .SetEase(_landingResetEase);
  }

  public void FixedUpdate(Player entity)
  {
  }

  public void Update(Player entity)
  {
    timeInState += Time.deltaTime;

    if (entity.Motor.IsGrounded && timeInState >= _groundedGraceTime)
    {
      entity.ActionLayer.ExitState(this, entity);
      return;
    }

    TrickDirection direction = entity.ConsumeTrickInput();

    if (direction != TrickDirection.None)
    {
      PerformFreeTrick(entity, direction);
      PushDirection(entity, direction);
    }

    if (buffer.Count == 0)
      return;

    bufferTimer += Time.deltaTime;

    if (bufferTimer >= _sequenceTimeout)
    {
      TrickSequence match = FindExactMatch();

      if (match != null)
        entity.AddScore(match.Score);

      buffer.Clear();
    }
  }

  void PerformFreeTrick(Player entity, TrickDirection direction)
  {
    entity.AddScore(_freeTrickScore);

    DirectionRotation config = FindRotation(direction);

    if (config == null)
      return;

    if (trickTween != null && trickTween.IsActive())
      trickTween.Complete();

    trickTween = entity.Model
      .DOLocalRotate(config.Rotation, config.Duration, RotateMode.LocalAxisAdd)
      .SetEase(config.Ease);
  }

  void PushDirection(Player entity, TrickDirection direction)
  {
    buffer.Add(direction);
    bufferTimer = 0f;

    if (!IsPrefixOfAny())
    {
      buffer.Clear();
      buffer.Add(direction);

      if (!IsPrefixOfAny())
      {
        buffer.Clear();
        return;
      }
    }

    TrickSequence match = FindExactMatch();

    if (match != null && !HasLongerSequence())
    {
      entity.AddScore(match.Score);
      buffer.Clear();
    }
  }

  DirectionRotation FindRotation(TrickDirection direction)
  {
    foreach (DirectionRotation rotation in _rotations)
    {
      if (rotation.Direction == direction)
        return rotation;
    }

    return null;
  }

  TrickSequence FindExactMatch()
  {
    TrickSequence best = null;

    foreach (TrickSequence sequence in _sequences)
    {
      if (sequence.Directions.Count != buffer.Count)
        continue;

      if (StartsWithBuffer(sequence))
        best = sequence;
    }

    return best;
  }

  bool IsPrefixOfAny()
  {
    foreach (TrickSequence sequence in _sequences)
    {
      if (sequence.Directions.Count >= buffer.Count && StartsWithBuffer(sequence))
        return true;
    }

    return false;
  }

  bool HasLongerSequence()
  {
    foreach (TrickSequence sequence in _sequences)
    {
      if (sequence.Directions.Count > buffer.Count && StartsWithBuffer(sequence))
        return true;
    }

    return false;
  }

  bool StartsWithBuffer(TrickSequence sequence)
  {
    for (int i = 0; i < buffer.Count; i++)
    {
      if (sequence.Directions[i] != buffer[i])
        return false;
    }

    return true;
  }
}
