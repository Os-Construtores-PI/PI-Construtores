using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class GlobalEventBus : MonoBehaviour
{
  public static GlobalEventBus Instance { get; private set; }

  public static bool HasInstance => Instance != null;

  #region Events
  public readonly PlayerComboEvent ComboUpdate = new();
  public readonly PlayerImpactEvent MaxComboReached = new();
  public readonly PlayerScoreEvent ScoreUpdate = new();
  public readonly PlayerLockOnEvent LockOnVisibility = new();
  public readonly PlayerDialogueEvent Dialogue = new();
  public readonly PlayerLockDlgEvent LockDialogue = new();
  public readonly PlayerSkipDlgEvent SkipDialogue = new();
  public readonly PlayerSkipDlgEvent EndDialogue = new();
  public readonly PlayerObjectSeenEvent ObjectWasSeen = new();
  public readonly PlayerCinematicEvent Cinematic = new();
  public readonly PlayerTeleportEvent Teleport = new();
  public readonly PlayerAmethystsEvent AmethystsChanged = new();
  public readonly UnityEvent Death = new();
  public readonly UnityEvent Respawn = new();
  public readonly UnityEvent EndGame = new();
  public readonly UnityEvent<bool> Pause = new();
  public readonly UnityEvent<bool> Options = new();
  public readonly UnityEvent<string> InputUpdate = new();
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
