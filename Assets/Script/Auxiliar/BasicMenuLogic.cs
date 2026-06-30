using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenuLogic : MonoBehaviour
{
  [Header("Audio")]
  [SerializeField]
  private somMenu somMenu;

  #region  === MENU GAMEOVER ===
  public void Respawn()
  {
    if (AudioManager.Instance != null && somMenu != null)
      AudioManager.Instance.PlaySFX(somMenu.gameOverMenu);
    GlobalEventBus.Instance.Respawn.Invoke();
  }

  public void OpenMenuGameOver()
  {
    if (AudioManager.Instance != null && somMenu != null)
    {
      AudioManager.Instance.PlaySFX(somMenu.gameOverMenu);
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
    AudioManager.Instance.PlaySFX(somMenu.click); // abrir options
  }

  public void ContinueGame()
  {
    GlobalEventBus.Instance.Pause.Invoke(false);
    AudioManager.Instance.PlaySFX(somMenu.click); // som de voltar
  }

  private void OnEnable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.Pause.AddListener(OnPauseChanged);
  }

  private void OnDisable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.Pause.RemoveListener(OnPauseChanged);
  }

  private void OnPauseChanged(bool isPaused)
  {
    if (AudioManager.Instance == null || somMenu == null)
      return;

    if (isPaused)
    {
      // 🔊 abriu pause
      AudioManager.Instance.PlaySFX(somMenu.pause);
    }
    else
    {
      // 🔊 fechou pause
      AudioManager.Instance.PlaySFX(somMenu.back);
    }
  }

  #endregion === MENU PAUSE ===

  #region  === COMUNS ===
  public void ExitToMainMenu()
  {
    Time.timeScale = 1f;

    DOTween.Kill(transform);

    GlobalEventBus.Instance.Pause.Invoke(false);

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();
    SceneManager.LoadScene(Constants.SceneNames.MainMenu);

    DOTween.KillAll();

    Resources.UnloadUnusedAssets();

    AudioManager.Instance.PlaySFX(somMenu.back);
  }
  #endregion
}
