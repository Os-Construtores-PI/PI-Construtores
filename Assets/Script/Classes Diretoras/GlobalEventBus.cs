using UnityEngine;
using UnityEngine.Events;

public class GlobalEventBus : MonoBehaviour
{
    private static GlobalEventBus _instance;

    public static GlobalEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tenta encontrar na cena antes de criar
                _instance = FindAnyObjectByType<GlobalEventBus>();
                if (_instance == null)
                {
                    var obj = new GameObject("GlobalEventBus");
                    _instance = obj.AddComponent<GlobalEventBus>();
                }
                _instance.Initialize();
            }
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    // --- Eventos ---
    [HideInInspector] public UnityEvent<bool, InteractableObject, int> ObjectWasSeen = new();
    [HideInInspector] public UnityEvent<int> TriggeredCinematic = new();
    [HideInInspector] public UnityEvent<int> AmethystsAmountChanged = new();
    private bool _initialized = false;

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
    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Persistente entre cenas
        DontDestroyOnLoad(gameObject);
    }
}

