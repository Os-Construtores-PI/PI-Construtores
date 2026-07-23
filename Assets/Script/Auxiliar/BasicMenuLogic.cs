using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenuLogic : MonoBehaviour
{
  [Header("Audio")]
  [SerializeField]
  private BackgroundMusicConfig _backgroundMusicConfig;

  [SerializeField]
  private UIAudioConfig _uiAudioConfig;

  #region  === MENU GAMEOVER ===
  public void Respawn()
  {
    if (AudioManager.Instance != null && _backgroundMusicConfig != null)
      AudioManager.Instance.PlaySFX(_backgroundMusicConfig.GameOverMusic);
    GlobalEventBus.Instance.Respawn.Invoke();
  }

  public void OpenMenuGameOver()
  {
    if (AudioManager.Instance != null && _backgroundMusicConfig != null)
    {
      AudioManager.Instance.PlaySFX(_backgroundMusicConfig.GameOverMusic);
    }
  }

  public void ResetScene()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
  #endregion === MENU GAMEOVER ===

  #region  === MENU PAUSE ===
  public void OpenOptions()
  {
    GlobalEventBus.Instance.Options.Invoke(true);
    AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  public void ContinueGame()
  {
    GlobalEventBus.Instance.Pause.Invoke(false);
    AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  public void OnEnable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.Pause.AddListener(OnPauseChanged);
  }

  public void OnDisable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.Pause.RemoveListener(OnPauseChanged);
  }

  private void OnPauseChanged(bool isPaused)
  {
    if (AudioManager.Instance == null || _uiAudioConfig == null)
      return;

    if (isPaused)
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);
    }
    else
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Back);
    }
  }

  #endregion === MENU PAUSE ===

  #region  === COMUNS ===
  public void ExitToMainMenu()
  {
    Time.timeScale = 1f;

   // DOTween.Kill(transform);

    GlobalEventBus.Instance.Pause.Invoke(false);

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();
    SceneManager.LoadScene(Constants.SceneNames.MainMenu);

    DOTween.KillAll();

    Resources.UnloadUnusedAssets();

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Back);
  }
  #endregion
}
