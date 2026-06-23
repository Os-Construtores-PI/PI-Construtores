using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class GlobalEventBus : MonoBehaviour
{
  public static GlobalEventBus Instance { get; private set; }

  public static bool HasInstance => Instance != null;

  #region Events
  public readonly UnityEvent<bool, InteractableObject, int> ObjectWasSeen = new();
  public readonly UnityEvent<int, float> Cinematic = new();
  public readonly UnityEvent<int> Teleport = new();
  public readonly UnityEvent Death = new();
  public readonly UnityEvent Respawn = new();
  public readonly UnityEvent EndGame = new();
  public readonly UnityEvent<int, int, ComboPopupType> ComboUpdate = new();
  public readonly UnityEvent<int, ImpactPopupType> MaxComboReached = new();
  public readonly UnityEvent<bool> Pause = new();
  public readonly UnityEvent<int, int> ScoreUpdate = new();
  public readonly UnityEvent<int, bool, Vector3> LockOnVisibility = new();
  public readonly UnityEvent<bool> Options = new();
  public readonly UnityEvent<int> AmethystsChanged = new();
  public readonly UnityEvent<string> InputChanged = new();
  public readonly UnityEvent<Player, List<string>, float> Dialogue = new();
  public readonly UnityEvent<Player, bool> LockDialogue = new();
  public readonly UnityEvent<Player> SkipDialogue = new();
  public readonly UnityEvent<Player> EndDialogue = new();
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
