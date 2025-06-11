using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsLogic : MonoBehaviour
{
    public void OnExitPressed()
    {
        SceneManager.LoadScene("Menu");
    }
}
