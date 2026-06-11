using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#region Utilities

[System.Serializable]
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
      return true;
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

  public void Exit()
  {
    if (exited || onExit == null)
      return;
    entered = false;
    exited = true;
    onExit.Invoke();
  }

  public void Check(bool condition)
  {
    if (condition)
      Enter();
    else
      Exit();
  }
}

#endregion

#region Audio

[Serializable]
public abstract class SFX
{
  public AudioClip Audio;
}

[Serializable]
public class PlayerSFX : SFX
{
  public PlayerAudioType Type;
}

public class SoundsWorker<TEnum>
  where TEnum : Enum
{
  private readonly Dictionary<TEnum, AudioClip> _sounds = new();
  private AudioSource _audioSource;

  public void Init<TSFX>(List<TSFX> soundsList, Func<TSFX, TEnum> keySelector, AudioSource source)
    where TSFX : SFX
  {
    foreach (var sfx in soundsList)
      _sounds[keySelector(sfx)] = sfx.Audio;

    _audioSource = source;
  }

  public void Play(TEnum key)
  {
    if (_sounds.TryGetValue(key, out var clip))
    {
      _audioSource.clip = clip;
      _audioSource.Play();
    }
  }

  public void Stop(TEnum key)
  {
    if (_sounds.TryGetValue(key, out var clip) && _audioSource.clip == clip)
    {
      _audioSource.clip = null;
      _audioSource.Stop();
    }
  }
}

#endregion

#region Effects


public class EffectsWorker
{
  private readonly Dictionary<EffectType, GameObject> effects = new();

  public void InitEffects(Transform transform)
  {
    effects.Clear();
    foreach (Transform child in transform)
    {
      if (Lookups.Effects.LookupTable.TryGetValue(child.tag, out EffectType effectType))
      {
        effects.Add(effectType, child.gameObject);
        StopEffect(effectType);
      }
    }
  }

  public void PlayEffect(EffectType effectType, float duration)
  {
    if (
      effects.TryGetValue(effectType, out GameObject effect)
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

  public void StopEffect(EffectType effectType)
  {
    if (
      effects.TryGetValue(effectType, out GameObject effect)
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
      light.enabled = state;
  }
}

public class TrailsWorker
{
  private readonly Dictionary<TrailType, GameObject> _trails = new();

  public void InitTrails(Transform parent)
  {
    _trails.Clear();
    foreach (Transform child in parent)
    {
      if (Lookups.Trails.LookupTable.TryGetValue(child.tag, out TrailType trailType))
      {
        _trails.Add(trailType, child.gameObject);
        StopEffect(trailType);
      }
    }
  }

  public void PlayEffect(TrailType trailType)
  {
    if (
      _trails.TryGetValue(trailType, out GameObject trail)
      && trail.TryGetComponent(out TrailRenderer trailRenderer)
    )
    {
      trailRenderer.Clear();
      trailRenderer.emitting = true;
    }
  }

  public void StopEffect(TrailType trailType)
  {
    if (
      _trails.TryGetValue(trailType, out GameObject trail)
      && trail.TryGetComponent(out TrailRenderer trailRenderer)
    )
    {
      trailRenderer.emitting = false;
    }
  }
}

#endregion


#region Puzzle

[Serializable]
public class ColorPuzzle
{
  public string id;
  public bool canFlash;
  public CodeCapturer codeCapturer;
  public float durationDesired;
  public List<PuzzleLampObject> lamps;
}

#endregion
