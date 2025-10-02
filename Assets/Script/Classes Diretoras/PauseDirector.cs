using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDirector : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI; // painel do pause
    [SerializeField] private string pauseMenuTag = "PauseMenu"; // Tag para identificar o painel


    private bool isPaused = false;
    public bool IsPaused => isPaused;
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
            TogglePause();
        }
    }
    private void TogglePause()
    {
        SetPause(!IsPaused);
    }
    public void SetPause(bool setPause)
    {
        pauseMenuUI.SetActive(setPause);
        Time.timeScale = setPause ? 0f : 1f;
        isPaused = setPause;
        Cursor.lockState = setPause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = setPause;
    }

    public void OpenOptions()
    {
        // aqui voc� pode abrir outro painel ou cena de op��es
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // garante que o tempo volte
        SceneManager.LoadScene(Constants.SceneNames.MenuScene);
    }
}
