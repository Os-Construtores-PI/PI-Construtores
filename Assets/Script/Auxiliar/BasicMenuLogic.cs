using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenuLogic : MonoBehaviour
{
    public void Respawn()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.Invoke();
    }
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ExitToMainMenu()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
        SceneManager.LoadScene(Constants.SceneNames.MainMenu);
    }
}
