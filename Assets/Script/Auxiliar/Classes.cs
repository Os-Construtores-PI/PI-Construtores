using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class ColorPuzzle
{
  public string id;
  public bool canFlash;
  public CodeCapturer codeCapturer;
  public float durationDesired;
  public List<PuzzleLampObject> lamps;
}

[Serializable]
public class CustomPositiveFloatRange
{
  [SerializeField, Range(0.01f, 10f)]
  private float min = 0.5f;

  [SerializeField, Range(0.01f, 10f)]
  private float max = 1.5f;

  private const float MIN_LIMIT = 0.01f;
  private const float MAX_LIMIT = 10f;

  public float Min
  {
    get => min;
    set
    {
      min = Mathf.Clamp(value, MIN_LIMIT, MAX_LIMIT);
      if (min > max)
        max = min;
    }
  }

  public float Max
  {
    get => max;
    set
    {
      max = Mathf.Clamp(value, MIN_LIMIT, MAX_LIMIT);
      if (max < min)
        min = max;
    }
  }

  public float GetRandom() => UnityEngine.Random.Range(min, max);

  public bool IsValid() => min >= MIN_LIMIT && max <= MAX_LIMIT && min <= max;
}

[System.Serializable]
public class Timer
{
  private float current;
  private float duration;
  private bool active;

  public bool IsActive => active;
  public bool IsDone => !active;

  public float Current => current;
  public float TimeLeft => duration - current;

  public void Start(float duration)
  {
    this.duration = duration;
    current = 0f;
    active = true;
  }

  public void Stop() => active = false;

  public bool Tick(float deltaTime)
  {
    if (!active)
      return false;
    current += deltaTime;
    if (current >= duration)
    {
      active = false;
      return true; // terminou
    }
    return false;
  }
}

[System.Serializable]
public class Scanner<TInput, TOutput>
{
  private readonly Func<TInput, TOutput> scanFunc;

  public Scanner(Func<TInput, TOutput> scanFunc)
  {
    this.scanFunc = scanFunc;
  }

  public (bool executed, TOutput result) Scan(TInput input)
  {
    return (true, scanFunc(input));
  }
}

[System.Serializable]
public class ConditionalGate
{
  bool entered = false;
  bool exited = false;
  Action onEnter;
  Action onExit;

  public void Setup(Action enterAction, Action exitAction)
  {
    onEnter = enterAction;
    onExit = exitAction;
  }

  public void Enter()
  {
    if (entered || onEnter == null)
      return;
    entered = true;
    exited = false;
    onEnter.Invoke();
  }

  public void Check(bool condition)
  {
    if (condition)
    {
      Enter();
    }
    else
    {
      Exit();
    }
  }

  public void Exit()
  {
    if (exited || onExit == null)
      return;
    entered = false;
    exited = true;
    onExit.Invoke();
  }
}

public class EffectsWorker
{
  private Dictionary<string, GameObject> effects = new();

  public void InitEffects(Transform transform)
  {
    effects.Clear();
    foreach (Transform child in transform)
    {
      effects.Add(child.name, child.gameObject);
    }
  }

  public void PlayEffect(string name, float duration)
  {
    if (
      effects.TryGetValue(name, out GameObject effect)
      && effect.TryGetComponent(out ParticleSystem particleSystem)
    )
    {
      particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      ParticleSystem.MainModule main = particleSystem.main;
      main.duration = duration;
      particleSystem.Play(true);

      SetLights(effect, true);
    }
  }

  public void StopEffect(string name)
  {
    if (
      effects.TryGetValue(name, out GameObject effect)
      && effect.TryGetComponent(out ParticleSystem particleSystem)
    )
    {
      particleSystem.Stop(true);

      SetLights(effect, false);
    }
  }

  private void SetLights(GameObject effect, bool state)
  {
    foreach (Light light in effect.GetComponentsInChildren<Light>(true))
    {
      light.enabled = state;
    }
  }
}

public class PressAndReleaseButton
{
  public PressAndReleaseButton(Player player)
  {
    _player = player;
  }

  protected Player _player;

  public virtual void Update() { }

  public virtual void OnInputAction(InputAction.CallbackContext context) { }
}

public class IncreaseButton : PressAndReleaseButton
{
  protected float _maxValue = 100;
  public float Value;
  private float _sumVelocity = 1f;
  private float _simpleActionInterval = 0.5f;
  private float _initialTime;
  private bool _isPressed = false;
  private float limit = 1.5f;

  public UnityEvent IsUsingEv = new();
  public UnityEvent StoppedUsingEv = new();

  public IncreaseButton(
    Player player,
    float maxValue,
    float sumVelocity,
    float simpleActionInterval
  )
    : base(player)
  {
    _maxValue = maxValue;
    _sumVelocity = sumVelocity;
    _simpleActionInterval = simpleActionInterval;
  }

  public override void Update()
  {
    if (_isPressed && _player.MovementVector.sqrMagnitude < Mathf.Pow(limit, 2))
    {
      if (Time.time - _initialTime >= _simpleActionInterval)
      {
        Value = Mathf.Min(Value + _sumVelocity * Time.deltaTime, _maxValue);
        Debug.Log(Value);
      }
    }
    else
    {
      StoppedUsingEv.Invoke();
    }
  }

  public override void OnInputAction(InputAction.CallbackContext context)
  {
    if (context.started)
    {
      _initialTime = Time.time;
      _isPressed = true;
      IsUsingEv.Invoke();
    }

    if (context.canceled)
    {
      float duration = Time.time - _initialTime;

      if (duration < _simpleActionInterval)
      {
        SimpleAction();
      }

      _isPressed = false;
      StoppedUsingEv.Invoke();
    }
  }

  protected virtual void SimpleAction() { }
}

public class BoostSlashDashButton : IncreaseButton
{
  public float SpeedMultiplier => Value > 0f ? 2 : 1f;

  public BoostSlashDashButton(
    Player player,
    float maxValue,
    float sumVelocity,
    float simpleActionInterval
  )
    : base(player, maxValue, sumVelocity, simpleActionInterval) { }

  protected override void SimpleAction()
  {
    if (
      !_player.CanDash
      || _player.CurrentDashCount >= _player.MaxDashCount
      || _player.IsDashBlocked
    )
      return;
    _player.ActionLayer.PushState(_player.DashAS, _player);
  }
}
