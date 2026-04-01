using UnityEngine;
using UnityEngine.Events;

public partial class TickDirector : MonoBehaviour
{
  private static TickDirector _instance;
  public static TickDirector Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance =
          FindAnyObjectByType<TickDirector>()
          ?? new GameObject("TickDirector").AddComponent<TickDirector>();
        _instance.Initialize();
      }
      return _instance;
    }
  }

  [HideInInspector]
  public UnityEvent<uint> OnTick = new();

  [HideInInspector]
  public UnityEvent<uint> OnFiveTick = new();

  [HideInInspector]
  public UnityEvent<uint> OnTenTick = new();

  [HideInInspector]
  public UnityEvent<uint> OnFifteenTick = new();

  [HideInInspector]
  public UnityEvent<uint> OnSecond = new();
  private const float TIMEPERTICK = .05f;

  private uint _tick;
  private bool _initialized;
  private float _tickTimer = 0f;

  private void Awake()
  {
    if (_instance != null && _instance != this)
    {
#if UNITY_EDITOR
      DestroyImmediate(gameObject);
#else
      Destroy(gameObject);
#endif
      return;
    }

    _instance = this;
    Initialize();
  }

  public void Update()
  {
    float delta = Time.deltaTime;
    _tickTimer += delta;
    if (_tickTimer > TIMEPERTICK)
    {
      _tickTimer -= TIMEPERTICK;
      _tick++;

      OnTick.Invoke(_tick);
      if (_tick % 5 == 0)
      {
        OnFiveTick.Invoke(_tick);
      }
      if (_tick % 10 == 0)
      {
        OnTenTick.Invoke(_tick);
      }
      if (_tick % 15 == 0)
      {
        OnFifteenTick.Invoke(_tick);
      }
      if (_tick % 20 == 0)
      {
        OnSecond.Invoke(_tick);
      }
    }
  }

  private void Initialize()
  {
    if (_initialized)
      return;
    _initialized = true;

    _tick = 0;
    DontDestroyOnLoad(gameObject); // persistente entre cenas
  }

  public uint GetCurrentTick() => _tick;
}
