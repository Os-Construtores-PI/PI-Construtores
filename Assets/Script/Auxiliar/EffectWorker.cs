using System;
using System.Threading;
using UnityEngine;

public class EffectWorker : MonoBehaviour
{
  [SerializeField]
  private EntityEffectType effectType;

  [SerializeField]
  private float defaultDuration = 1f;

  private ParticleSystem[] _systems;
  private Light[] _lights;
  private CancellationTokenSource _cts;

  public EntityEffectType EffectType => effectType;
  public bool IsPlaying => _cts != null;

  private void Awake()
  {
    Cache();
    StopSystems();
  }

  private void OnDestroy()
  {
    CancelToken();
  }

  [ContextMenu("Recache")]
  private void Cache()
  {
    _systems = GetComponentsInChildren<ParticleSystem>(true);
    _lights = GetComponentsInChildren<Light>(true);

    foreach (ParticleSystem ps in _systems)
    {
      ParticleSystem.MainModule main = ps.main;
      main.playOnAwake = false;
    }
  }

  public async void Play(float duration = 0f, Action onComplete = null)
  {
    if (_systems == null)
      Cache();

    if (_systems.Length == 0 || !isActiveAndEnabled)
      return;

    CancelToken();
    StopSystems();

    CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
      destroyCancellationToken
    );
    _cts = cts;
    float time = duration > 0f ? duration : defaultDuration;

    try
    {
      await Awaitable.NextFrameAsync(cts.Token);

      foreach (ParticleSystem ps in _systems)
      {
        if (!ps)
          continue;

        ps.Clear(false);
        ps.Simulate(0f, false, true);
        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = time;
        main.loop = false;

        ps.Play(false);
      }

      SetLights(true);

      await Awaitable.WaitForSecondsAsync(time, cts.Token);
    }
    catch (OperationCanceledException)
    {
      return;
    }

    if (_cts != cts)
      return;

    CancelToken();
    StopSystems();
    onComplete?.Invoke();
  }

  public void Stop()
  {
    if (_systems == null)
      Cache();

    CancelToken();
    StopSystems();
  }

  private void CancelToken()
  {
    if (_cts == null)
      return;

    _cts.Cancel();
    _cts.Dispose();
    _cts = null;
  }

  private void StopSystems()
  {
    foreach (ParticleSystem ps in _systems)
    {
      if (!ps)
        continue;

      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      ps.Clear(true);
      ps.Simulate(0f, true, true);
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    SetLights(false);
  }

  private void SetLights(bool state)
  {
    foreach (Light light in _lights)
      if (light)
        light.enabled = state;
  }
}
