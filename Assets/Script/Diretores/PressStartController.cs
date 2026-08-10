using UnityEngine;
using UnityEngine.InputSystem;

public class PressStartController : MonoBehaviour
{
    [SerializeField] private LoadingScreen loadingScreen;

    [SerializeField] private GameObject pressStartPanel;

    [SerializeField] private string menuScene = "MainMenu";


    private bool stated;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        if (!stated)
        {
            bool keyboard = 
                Keyboard.current != null &&
                Keyboard.current.anyKey.wasPressedThisFrame;
            
            bool gamepad =
                Gamepad.current != null &&(
                    Gamepad.current.startButton.wasPressedThisFrame ||
                    Gamepad.current.startButton.wasPressedThisFrame
                );
            
            if(keyboard || gamepad)
            {
                stated = true;

                pressStartPanel.SetActive(false);

                loadingScreen.LoadScene(menuScene);
            }

        }
        
    }

    public void PressStartButton()
    {
        if (stated)
            return;

        stated = true;

        pressStartPanel.SetActive(false);

        loadingScreen.LoadScene(menuScene);
    }

    
}
