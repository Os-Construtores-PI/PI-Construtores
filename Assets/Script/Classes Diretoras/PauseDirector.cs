using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDirector : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenuUI; // painel do pause
    [SerializeField] private string pauseMenuTag = "PauseMenu"; // Tag para identificar o painel
    public string _mainMenuSceneName = "MainMenu"; // nome da cena principal


    public bool _isPause = false;


    // Update is called once per frame

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
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (_isPause) 
                Resume();
            else
                Pause();

        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        _isPause = false;

        // travar o cursor de novo no jogo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        _isPause=true;

        // libera o cursor para clicar nos bot�es
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenOptions()
    {
        Debug.Log("Abrir op��es ... (implementar menu de op��es aqui)");
        // aqui voc� pode abrir outro painel ou cena de op��es
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // garante que o tempo volte
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
