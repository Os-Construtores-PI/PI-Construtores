using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenuLogic : MonoBehaviour
{
  [Header("Audio")]
  [SerializeField] private somMenu somMenu;

  #region  === MENU GAMEOVER ===
  public void Respawn()
  {
    GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.Invoke();
  }

  public void ResetScene()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
  #endregion === MENU GAMEOVER ===

  #region  === MENU PAUSE ===
  public void OpenOptions()
  {
    //AudioManager.Instance.PlaySFX(somMenu.click); // abrir options
    GlobalEventBus.Instance.PLAYERTRIGGEREDOPTIONS.Invoke(true);
  }

  public void ContinueGame()
  {
   // AudioManager.Instance.PlaySFX(somMenu.click); // som de voltar
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
  }

  

  #endregion === MENU PAUSE ===

  #region  === COMUNS ===
  public void ExitToMainMenu()
  {
    //AudioManager.Instance.PlaySFX(somMenu.back);

    Time.timeScale = 1f;
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();
    SceneManager.LoadScene(Constants.SceneNames.MainMenu);
  }
  #endregion
}
