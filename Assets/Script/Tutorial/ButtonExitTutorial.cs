using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonExitTutorial : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private Image buttonIcon;
    private PlayerInput playerInput;

    [Header("Sprites Por Device")]
    [SerializeField] private Sprite KeyboardIcon;  //F
    [SerializeField] private Sprite XboxIcon;  //A
    [SerializeField] private Sprite PlaystationIcon; // X

    private void OnEnable()
    {
        if (PlayerInput.all.Count > 0)
            playerInput = PlayerInput.all[0];

        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged += AtualizarIcone;

        if (TutorialGlobal.Instance != null)
            TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

        AtualizarIcone(DeviceSpriteManager.Instance?.GetCurrentDevice());
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        if (playerInput == null) return;
        if (TutorialGlobal.Instance == null) return;
        if (!TutorialGlobal.Instance.IsTutorialActive) return;

        // Ação única (F / A / X)
        if (playerInput.actions["Confirm"].WasPerformedThisFrame())
        {
            ClosedTutorial();
        }
    }
    public void ClosedTutorial()
    {
        TutorialGlobal.Instance.FecharTutorial();
    }

    private void OnTutorialStateChanged(bool ativo)
    {
        if(buttonIcon != null)
            buttonIcon.enabled = ativo;

        if(ativo)
            AtualizarIcone(DeviceSpriteManager.Instance?.GetCurrentDevice());
    }

    private void AtualizarIcone(string device)
    {
        if (buttonIcon == null) return;
        if(TutorialGlobal.Instance == null) return;
        if(!TutorialGlobal.Instance.IsTutorialActive) return;

        buttonIcon.sprite = device switch
        {
            "Keyboard" => KeyboardIcon,
            "Xbox" => XboxIcon,
            "Playstation" => PlaystationIcon,
            _ => buttonIcon.sprite
        };
    }
}
