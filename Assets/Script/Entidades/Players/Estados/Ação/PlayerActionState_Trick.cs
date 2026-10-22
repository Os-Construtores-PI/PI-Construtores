using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class PlayerActionStateTrick : IPlayerState<Player>
{
  public enum TrickDirection
  {
    None,
    Left,
    Up,
    Right,
    Down,
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
    new DirectionRotation
    {
      Direction = TrickDirection.Left,
      Rotation = new Vector3(0f, -360f, 0f),
    },
    new DirectionRotation
    {
      Direction = TrickDirection.Right,
      Rotation = new Vector3(0f, 360f, 0f),
    },
    new DirectionRotation { Direction = TrickDirection.Up, Rotation = new Vector3(360f, 0f, 0f) },
    new DirectionRotation
    {
      Direction = TrickDirection.Down,
      Rotation = new Vector3(-360f, 0f, 0f),
    },
  };

  [SerializeField]
  private List<TrickSequence> _sequences = new();

  public PlayerActionType Type => PlayerActionType.Trick;

  public HashSet<PlayerActionType> IncompatibleActions => new() { PlayerActionType.RailSlide };

  readonly List<TrickDirection> _buffer = new();
  Tween _trickTween;
  Tween _resetTween;
  Quaternion _restRotation;
  float _bufferTimer;
  float _timeInState;

  [SerializeField]
  private int _comboPerTrick = 3;
  int _trickCount = 0;

  public void Enter(Player entity)
  {
    if (_resetTween != null && _resetTween.IsActive())
      _resetTween.Complete();

    _restRotation = entity.Model.localRotation;
    _buffer.Clear();
    _bufferTimer = 0f;
    _trickCount = 0;
    _timeInState = 0f;
  }

  public void Exit(Player entity)
  {
    _buffer.Clear();

    if (_trickTween != null && _trickTween.IsActive())
      _trickTween.Kill();

    if (_landingResetDuration <= 0f)
    {
      entity.Model.localRotation = _restRotation;
      return;
    }

    _resetTween = entity
      .Model.DOLocalRotateQuaternion(_restRotation, _landingResetDuration)
      .SetEase(_landingResetEase);
  }

  public void FixedUpdate(Player entity) { }

  public void Update(Player entity)
  {
    _timeInState += Time.deltaTime;

    if (entity.Motor.IsGrounded && _timeInState >= _groundedGraceTime)
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

    if (_buffer.Count == 0)
      return;

    _bufferTimer += Time.deltaTime;

    if (_bufferTimer >= _sequenceTimeout)
    {
      TrickSequence match = FindExactMatch();

      if (match != null)
        entity.AddScore(match.Score);

      _buffer.Clear();
    }
  }

  void PerformFreeTrick(Player entity, TrickDirection direction)
  {
    entity.AddScore(_freeTrickScore);
    entity.AddBoostStage();
    _trickCount += 1;

    if (_trickCount % _comboPerTrick == 0)
    {
      entity.ComboUp();
    }

    DirectionRotation config = FindRotation(direction);

    if (config == null)
      return;

    if (_trickTween != null && _trickTween.IsActive())
      _trickTween.Complete();

    _trickTween = entity
      .Model.DOLocalRotate(config.Rotation, config.Duration, RotateMode.LocalAxisAdd)
      .SetEase(config.Ease);
  }

  void PushDirection(Player entity, TrickDirection direction)
  {
    _buffer.Add(direction);
    _bufferTimer = 0f;

    if (!IsPrefixOfAny())
    {
      _buffer.Clear();
      _buffer.Add(direction);

      if (!IsPrefixOfAny())
      {
        _buffer.Clear();
        return;
      }
    }

    TrickSequence match = FindExactMatch();

    if (match != null && !HasLongerSequence())
    {
      entity.AddScore(match.Score);
      entity.ComboUp();
      _buffer.Clear();
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
      if (sequence.Directions.Count != _buffer.Count)
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
      if (sequence.Directions.Count >= _buffer.Count && StartsWithBuffer(sequence))
        return true;
    }

    return false;
  }

  bool HasLongerSequence()
  {
    foreach (TrickSequence sequence in _sequences)
    {
      if (sequence.Directions.Count > _buffer.Count && StartsWithBuffer(sequence))
        return true;
    }

    return false;
  }

  bool StartsWithBuffer(TrickSequence sequence)
  {
    for (int i = 0; i < _buffer.Count; i++)
    {
      if (sequence.Directions[i] != _buffer[i])
        return false;
    }

    return true;
  }
}
