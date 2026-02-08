using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialGlobal : MonoBehaviour
{
    public static TutorialGlobal Instance;

    [Header("Ui")]
    [SerializeField] private GameObject tutorialHUD;

    [Header("Tutoriais")]
    [SerializeField] private GameObject movimentoTutorial;
    [SerializeField] private GameObject dashTutorial;
    
    public event System.Action<bool> OnTutorialStateChanged;

    
    public bool IsTutorialActive { get; private set; }

    private PlayerInput _playerInput;

    private Tween currentTween;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //tutorialHUD.SetActive(false);
        DesativarTodos();

    }
    private void Start()
    {
        tutorialHUD.SetActive(false);
    }

    public void AbrirTutorial(TutorialTrigger.TutorialType tipo)
    {
        if (IsTutorialActive) return;
        
        IsTutorialActive = true;


        DeviceSpriteManager.Instance?.ForceRefresh();

        DesativarTodos();
        AtivarTutorial(tipo);
        
        tutorialHUD.SetActive(true);

        OnTutorialStateChanged?.Invoke(true);

        
    }

    public void FecharTutorial()
    {
        if (!IsTutorialActive) return;

        IsTutorialActive = false;
        tutorialHUD.SetActive(false);
        DeviceSpriteManager.Instance?.ForceRefresh();
        OnTutorialStateChanged?.Invoke(false);
    }

    

    private void DesativarTodos()
    {
        if(movimentoTutorial != null) movimentoTutorial.SetActive(false);
        if(dashTutorial != null) dashTutorial.SetActive(false);

    }
    private void AtivarTutorial(TutorialTrigger.TutorialType tipo)
    {
        
        
        switch (tipo)
        {
            case TutorialTrigger.TutorialType.Movimento:
                movimentoTutorial.SetActive(true);
                break;
            case TutorialTrigger.TutorialType.Dash:
                dashTutorial.SetActive(true);
                break;
            
            
        }
    }
}
