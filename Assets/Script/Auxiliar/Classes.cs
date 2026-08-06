using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
  private readonly Dictionary<EntityEffectType, GameObject> effects = new();
  private readonly Dictionary<EntityEffectType, CancellationTokenSource> activeTokens = new();

  public void InitEffects(Transform transform)
  {
    effects.Clear();
    CancelAllTokens();

    foreach (Transform child in transform)
    {
      if (Lookups.Effects.LookupTable.TryGetValue(child.tag, out EntityEffectType effectType))
      {
        effects.Add(effectType, child.gameObject);

        foreach (ParticleSystem ps in child.GetComponentsInChildren<ParticleSystem>(true))
        {
          ParticleSystem.MainModule main = ps.main;
          main.playOnAwake = false;
        }

        StopAndClear(effectType);
      }
    }
  }

  public async void PlayEffect(
    EntityEffectType effectType,
    float duration,
    Action onComplete = null
  )
  {
    if (
      !effects.TryGetValue(effectType, out GameObject effect)
      || !effect.TryGetComponent(out ParticleSystem particleSystem)
    )
      return;

    CancelToken(effectType);

    var cts = new CancellationTokenSource();
    activeTokens[effectType] = cts;

    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    await Task.Yield();
    particleSystem.Clear(true);
    particleSystem.Simulate(0f, true, true);
    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystem.MainModule main = particleSystem.main;
    main.duration = duration;
    main.loop = false;

    particleSystem.Play(true);
    SetLights(effect, true);

    try
    {
      await Task.Delay(TimeSpan.FromSeconds(duration), cts.Token);
    }
    catch (OperationCanceledException)
    {
      return;
    }

    if (cts.IsCancellationRequested)
      return;

    activeTokens.Remove(effectType);
    StopAndClear(effectType);
    onComplete?.Invoke();
  }

  public void StopEffect(EntityEffectType effectType)
  {
    CancelToken(effectType);
    StopAndClear(effectType);
  }

  public void ResetEffect(EntityEffectType effectType)
  {
    CancelToken(effectType);
    StopAndClear(effectType);
  }

  private void StopAndClear(EntityEffectType effectType)
  {
    if (!effects.TryGetValue(effectType, out GameObject effect))
      return;

    foreach (ParticleSystem ps in effect.GetComponentsInChildren<ParticleSystem>(true))
    {
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      ps.Clear(true);
      ps.Simulate(0f, true, true);
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    SetLights(effect, false);
  }

  private void CancelToken(EntityEffectType effectType)
  {
    if (activeTokens.TryGetValue(effectType, out CancellationTokenSource cts))
    {
      cts.Cancel();
      cts.Dispose();
      activeTokens.Remove(effectType);
    }
  }

  private void CancelAllTokens()
  {
    foreach (var cts in activeTokens.Values)
    {
      cts.Cancel();
      cts.Dispose();
    }
    activeTokens.Clear();
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


#region extensions

public sealed class HudPanelEqualityComparer : IEqualityComparer<HudPanelType>
{
  public static readonly HudPanelEqualityComparer Instance = new();

  private HudPanelEqualityComparer() { }

  public bool Equals(HudPanelType x, HudPanelType y) => x == y;

  public int GetHashCode(HudPanelType obj) => (int)obj;
}

#endregion


#region Eventos

public class PlayerComboEvent : UnityEvent<int, int, ComboPopupType> { }

public class PlayerImpactEvent : UnityEvent<int, ImpactPopupType> { }

public class PlayerScoreEvent : UnityEvent<int, int> { }

public class PlayerLockOnEvent : UnityEvent<int, bool, Vector3> { }

public class PlayerDialogueEvent : UnityEvent<Player, List<string>, float> { }

public class PlayerLockDlgEvent : UnityEvent<Player, bool> { }

public class PlayerSkipDlgEvent : UnityEvent<Player> { }

public class PlayerObjectSeenEvent : UnityEvent<int, bool, InteractableObject> { }

public class PlayerCinematicEvent : UnityEvent<int, float> { }

public class PlayerTeleportEvent : UnityEvent<int> { }

public class PlayerAmethystsEvent : UnityEvent<int> { }

#endregion
