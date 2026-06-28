using UnityEngine;

public class PauseManagerOnAndOff : MonoBehaviour
{
  private void OnEnable()
  {
    GlobalEventBus.Instance.Pause.AddListener(SetPause);
  }

  private void OnDisable()
  {
    if (GlobalEventBus.HasInstance)
      GlobalEventBus.Instance.Pause.RemoveListener(SetPause);
  }

  private void SetPause(bool state)
  {
    GameContext.IsPaused = state;

    Time.timeScale = state ? 0f : 1f;

    //Debug.Log("Pause state:" + state);
  }
}
