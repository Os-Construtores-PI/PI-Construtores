using UnityEngine;
using UnityEngine.Events;

public class GlobalEventBus : MonoBehaviour
{
    public static GlobalEventBus Instance { get; private set; }
    [HideInInspector] public UnityEvent<bool,InteractableObject, int> ObjectWasSeen = new();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
