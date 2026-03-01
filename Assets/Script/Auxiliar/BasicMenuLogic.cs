using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenuLogic : MonoBehaviour
{

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
        GlobalEventBus.Instance.PLAYERTRIGGEREDOPTIONS.Invoke(true);
    }
    public void ContinueGame()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
    }

    #endregion === MENU PAUSE ===

    #region  === COMUNS ===
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);

        if(DataDirector.Instance != null)
           DataDirector.Instance.ResetRunTimeState();
        SceneManager.LoadScene(Constants.SceneNames.MainMenu);
    }
    #endregion
}
