using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverLogic : MonoBehaviour
{
    public void Respawn()
    {
        GlobalEventBus.Instance.PLAYERTRIGGERREDRESPAWN.Invoke();
    }
    public void ExitToMainMenu()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
        SceneManager.LoadScene(Constants.SceneNames.MainMenu);
    }
}
