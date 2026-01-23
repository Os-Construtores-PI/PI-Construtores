using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialGlobal : MonoBehaviour
{
    public static TutorialGlobal Instance;

    [Header("Ui")]
    [SerializeField] private GameObject tutorialHUD;

    
    public bool IsTutorialActive { get; private set; }

    private PlayerInput _playerInput;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tutorialHUD != null)
            tutorialHUD.SetActive(false);
    }

    public void AbrirTutorial(PlayerInput input)
    {
        if (IsTutorialActive) return;
        
        IsTutorialActive = true;

        tutorialHUD.SetActive(true);
    }

    public void FecharTutorial()
    {
        if (!IsTutorialActive) return;

        IsTutorialActive = false;
        tutorialHUD.SetActive(false);
    }
}
