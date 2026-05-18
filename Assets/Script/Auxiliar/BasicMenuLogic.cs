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
    GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.Invoke();
  }

  public void OpenMenuGameOver()
  {
    if(AudioManager.Instance != null && somMenu != null)
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
    GlobalEventBus.Instance.PLAYERTRIGGEREDOPTIONS.Invoke(true);
    AudioManager.Instance.PlaySFX(somMenu.click); // abrir options
  }

  public void ContinueGame()
  {
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
    AudioManager.Instance.PlaySFX(somMenu.click); // som de voltar
  }

  private void OnEnable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.AddListener(OnPauseChanged);
  }

  private void OnDisable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.RemoveListener(OnPauseChanged);
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
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();
    SceneManager.LoadScene(Constants.SceneNames.MainMenu);

    AudioManager.Instance.PlaySFX(somMenu.back);
  }
  #endregion
}
