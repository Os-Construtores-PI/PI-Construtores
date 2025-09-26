using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDirector : MonoBehaviour
{
    public GameObject pauseMenuUI; // painel do pause
    public string _mainMenuSceneName = "MainMenu"; // nome da cena principal


    public bool _isPause = false;


    // Update is called once per frame

    private void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
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

        // libera o cursor para clicar nos botões
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenOptions()
    {
        Debug.Log("Abrir opções ... (implementar menu de opções aqui)");
        // aqui você pode abrir outro painel ou cena de opções
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // garante que o tempo volte
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
