using UnityEngine;
using UnityEngine.Events;


[DefaultExecutionOrder(-100)]
public class GlobalEventBus : MonoBehaviour
{
    private static GlobalEventBus _instance;
    private bool _initialized;

    public static GlobalEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GlobalEventBus>() ??
                            new GameObject("GlobalEventBus").AddComponent<GlobalEventBus>();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    #region Events
    public readonly UnityEvent<bool, InteractableObject, int> OBJECTWASSEEN = new();
    public readonly UnityEvent<int, float> TRIGGEREDCINEMATIC = new();
    public readonly UnityEvent<int> TRIGGEREDTELEPORT = new();
    public readonly UnityEvent<Player> PLAYERTRIGGEREDDEATH = new();
    public readonly UnityEvent<int> AMETHYSTSAMOUNTCHANGED = new();
    #endregion

    #region Unity Lifecycle
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
    #endregion

    #region Private
    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        DontDestroyOnLoad(gameObject); // persistente entre cenas
    }
    #endregion
}
