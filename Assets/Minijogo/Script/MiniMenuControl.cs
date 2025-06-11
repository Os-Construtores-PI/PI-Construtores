using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MiniMenuControl : MonoBehaviour
{
    [SerializeField] private MiniGameControl MGC;
    private SaveSystem saveSystem = new();
    private AudioSource audioSource;

    [SerializeField] TextMeshProUGUI TextColor;
    [SerializeField] TextMeshProUGUI mainText;
    [SerializeField] TextMeshProUGUI ScoreText;
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] TextMeshProUGUI BestScoreText; // Death
    [SerializeField] TextMeshProUGUI BestScoreText1;

    [SerializeField] GameObject mainTextPanel;
    [SerializeField] GameObject HUDPanel;
    [SerializeField] GameObject DeathPanel;
    [SerializeField] GameObject WinPanel;
    [SerializeField] GameObject PauseP;
    


    [SerializeField] Button WinButton;
    [SerializeField] Button ResetButton;
    [SerializeField] Button MenuButton;
    [SerializeField] Button OptionsButton;
    [SerializeField] Button ExitButton;
    [SerializeField] Button PauseButton;

    void Start()
    {
        MGC = GetComponent<MiniGameControl>();
        GameObject.FindWithTag("MiniSoundController").TryGetComponent(out audioSource);
        LoadScore();
    }
    public void LoadScore()
    {
        int? last_pontuation = saveSystem.LoadInt("score");
        if (last_pontuation != null)
        {
            BestScoreText.text = "Melhor pontuação: " + last_pontuation;
            BestScoreText1.text = "Melhor pontuação: " + last_pontuation;
        }
        else
        {
            BestScoreText.gameObject.SetActive(false);
            BestScoreText1.gameObject.SetActive(false);
        }
    }
    public void DeathMessage()
    {
        SetActivePanel(DeathPanel);
    }
    public void WinMessage()
    {
        SetActivePanel(WinPanel);
        SetMainTexto("Você ganhou! Tente novamente ou vá para o menu!");

    }
    public void Pause()
    {
        Time.timeScale = 0;
        PauseP.SetActive(true);
        PauseButton.gameObject.SetActive(false);
        LoadScore();
    }
    public void UnPause()
    {
        PauseP.SetActive(false);
        PauseButton.gameObject.SetActive(true);
        Time.timeScale = 1;
    }
    public void ExitApp()
    {
        Application.Quit();
    }
    public void UI_Control(bool setDeath, bool setWin)
    {
        WinButton.gameObject.SetActive(setWin);
        MenuButton.gameObject.SetActive(setWin);
        ExitButton.gameObject.SetActive(setDeath);
        ResetButton.gameObject.SetActive(setDeath);
    }
    public void UI_HUDStart()
    {
        SetActivePanel(HUDPanel);
        mainText.gameObject.SetActive(false);
    }
    public void UI_UpdateHUD_Color(int id)
    {
        TextColor.text = "Próxima: " + MGC.nameColors[id];
        TextColor.color = MGC.colors[MGC.nameColors[id]];
    }
    public void UI_UpdateHUD_Health(int health)
    {
        HealthText.text = "Vida: " + health;
    }
    public void UI_UpdateHUD_Score(int score)
    {
        ScoreText.text = "Pontos: " + score;
    }
    public void FinishedLoading()
    {
        PauseButton.gameObject.SetActive(true);
        MGC.StartGame();
        audioSource.Play();
    }
    private void SetActivePanel(GameObject activePanel)
    {
        HUDPanel.SetActive(activePanel == HUDPanel);
        DeathPanel.SetActive(activePanel == DeathPanel);
        WinPanel.SetActive(activePanel == WinPanel);
        //OptionsButton.SetActive(activePanel == OptionsButton);

    }
    private void SetMainTexto(string message)
    {
        mainTextPanel.SetActive(true);
        mainText.gameObject.SetActive(true);
        mainText.text = message;
    }
}
