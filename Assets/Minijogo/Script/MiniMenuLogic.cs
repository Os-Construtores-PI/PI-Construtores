using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniMenuLogic : MonoBehaviour
{
    [SerializeField] GameObject Sound;

    [SerializeField] Button _soundbutton;
    public void OnPlayPressed()
    {
        SceneManager.LoadScene("CenaMinijogo");
    }
    public void OnExitPressed()
    {
        Application.Quit();
    }

    public void OnSoundPressed()
    {
        _soundbutton.gameObject.SetActive(Sound);
    }
    public void OnCreditsPressed()
    {
        SceneManager.LoadScene("Credits");
    }
    

   
}
