using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDirector : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI; // painel do pause
    [SerializeField] private string pauseMenuTag = "PauseMenu"; // Tag para identificar o painel

    private void Start()
    {
        GameObject foundPause = GameObject.FindGameObjectWithTag(pauseMenuTag);

        if (foundPause != null)
        {

            pauseMenuUI = foundPause;
            pauseMenuUI.SetActive(false);
        }

        else
        {
            Debug.LogWarning($"[PauseDirector] nenhum objeto com a TAG '{pauseMenuTag}' foi encontrado");
        }
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.AddListener(SetPauseMenu);
    }

    public void SetPauseMenu(bool setPause)
    {
        pauseMenuUI.SetActive(setPause);
        Cursor.lockState = setPause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = setPause;
    }
    
    public void ContinueGame()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(false);
    }

    public void OpenOptions()
    {
        // aqui voc� pode abrir outro painel ou cena de op��es
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // garante que o tempo volte
        SceneManager.LoadScene(Constants.SceneNames.MainMenu);
    }
}
