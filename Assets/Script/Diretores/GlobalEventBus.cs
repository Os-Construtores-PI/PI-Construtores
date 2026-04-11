using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class GlobalEventBus : MonoBehaviour
{
  public static GlobalEventBus Instance { get; private set; }

  public static bool HasInstance => Instance != null;

  #region Events
  public readonly UnityEvent<bool, InteractableObject, int> OBJECTWASSEEN = new();
  public readonly UnityEvent<int, float> PLAYERTRIGGEREDCINEMATIC = new();
  public readonly UnityEvent<int> PLAYERTRIGGEREDTELEPORT = new();
  public readonly UnityEvent PLAYERTRIGGEREDDEATH = new();
  public readonly UnityEvent PLAYERTRIGGEREDRESPAWN = new();
  public readonly UnityEvent PLAYERTRIGGEREDENDGAME = new();
  public readonly UnityEvent<bool> PLAYERTRIGGEREDPAUSE = new();
  public readonly UnityEvent<bool> PLAYERTRIGGEREDOPTIONS = new();
  public readonly UnityEvent<int, Vector3?> AMETHYSTSAMOUNTCHANGED = new();
  public readonly UnityEvent<string> PLAYERINPUTCHANGED = new();
  public readonly UnityEvent<Player, List<string>, float> PLAYERTRIGGEREDDIALOGUE = new();
  public readonly UnityEvent<Player, bool> PLAYERTRIGGEREDLOCKDIALOGUE = new();
  public readonly UnityEvent<Player> PLAYERTRIGGEREDSKIPDIALOGUE = new();
  public readonly UnityEvent<Player> PLAYERTRIGGEREDENDDIALOGUE = new();
  #endregion

  public void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }
  //#endregion
}
