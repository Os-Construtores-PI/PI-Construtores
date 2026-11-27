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

#region Extension

public static class Extensions
{
  public static bool TryFindInParents<T>(
    this Transform origin,
    out T result,
    int maxLevels = int.MaxValue,
    Func<T, bool> filter = null
  )
    where T : Component
  {
    Transform current = origin.parent;
    int level = 0;

    while (current != null && level < maxLevels)
    {
      if (current.TryGetComponent(out T found) && (filter == null || filter(found)))
      {
        result = found;
        return true;
      }

      current = current.parent;
      level++;
    }

    result = null;
    return false;
  }

  public static void FindInChildren<T>(
    this Transform origin,
    List<T> results,
    bool includeInactive = false,
    bool stopAtMatch = true,
    int maxDepth = int.MaxValue,
    Func<T, bool> filter = null
  )
    where T : Component
  {
    if (maxDepth <= 0)
    {
      return;
    }

    for (int i = 0; i < origin.childCount; i++)
    {
      Transform child = origin.GetChild(i);

      if (!includeInactive && !child.gameObject.activeSelf)
      {
        continue;
      }

      bool matched = false;

      if (child.TryGetComponent(out T found) && (filter == null || filter(found)))
      {
        results.Add(found);
        matched = true;
      }

      if (matched && stopAtMatch)
      {
        continue;
      }

      child.FindInChildren(results, includeInactive, stopAtMatch, maxDepth - 1, filter);
    }
  }

  public static bool TryFindNearestInChildren<T>(
    this Transform origin,
    out T result,
    bool includeInactive = false,
    int maxDepth = int.MaxValue
  )
    where T : Component
  {
    Queue<(Transform node, int depth)> queue = new();
    queue.Enqueue((origin, 0));

    while (queue.Count > 0)
    {
      (Transform node, int depth) = queue.Dequeue();

      if (depth >= maxDepth)
      {
        continue;
      }

      for (int i = 0; i < node.childCount; i++)
      {
        Transform child = node.GetChild(i);

        if (!includeInactive && !child.gameObject.activeSelf)
        {
          continue;
        }

        if (child.TryGetComponent(out result))
        {
          return true;
        }

        queue.Enqueue((child, depth + 1));
      }
    }

    result = null;
    return false;
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
  private readonly Dictionary<EntityEffectType, EffectWorker> _effects = new();

  public void InitEffects(Transform parent)
  {
    _effects.Clear();

    foreach (Transform child in parent)
    {
      if (!child.TryGetComponent(out EffectWorker worker))
        continue;

      if (!_effects.TryAdd(worker.EffectType, worker))
        Debug.LogWarning($"Efeito duplicado {worker.EffectType} em {parent.name}", child);
    }
  }

  public void PlayEffect(EntityEffectType type, float duration = 0f, Action onComplete = null)
  {
    if (_effects.TryGetValue(type, out EffectWorker worker))
      worker.Play(duration, onComplete);
  }

  public void StopEffect(EntityEffectType type)
  {
    if (_effects.TryGetValue(type, out EffectWorker worker))
      worker.Stop();
  }

  public void StopAllEffects()
  {
    foreach (EffectWorker worker in _effects.Values)
      if (worker)
        worker.Stop();
  }
}

public class TrailsWorker
{
  private readonly Dictionary<TrailType, TrailWorker> _trails = new();

  public void InitTrails(Transform parent)
  {
    _trails.Clear();

    foreach (Transform child in parent)
    {
      if (!child.TryGetComponent(out TrailWorker worker))
        continue;

      if (!_trails.TryAdd(worker.TrailType, worker))
      {
        Debug.LogWarning($"Trail duplicado {worker.TrailType} em {parent.name}", child);
        continue;
      }

      worker.StopTrail();
    }
  }

  public void PlayEffect(TrailType trailType)
  {
    if (_trails.TryGetValue(trailType, out TrailWorker worker) && worker)
      worker.PlayTrail();
  }

  public void StopEffect(TrailType trailType)
  {
    if (_trails.TryGetValue(trailType, out TrailWorker worker) && worker)
      worker.StopTrail();
  }

  public void ResetEffect(TrailType trailType)
  {
    if (_trails.TryGetValue(trailType, out TrailWorker worker) && worker)
      worker.ClearTrail();
  }

  public void ResetAll()
  {
    foreach (TrailWorker worker in _trails.Values)
      if (worker)
        worker.ClearTrail();
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

public class PlayerTimeStopEvent : UnityEvent<int, bool> { }

#endregion
