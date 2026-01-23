using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialTrigger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ImageTriggerEvent interactionIcon;
    [SerializeField] private Image interactionSprite;

    [Header("Config")]
    [SerializeField] private bool apenasUmaVez;

    private PlayerInput playerInput;
    private bool jogadorDentro;
    private bool tutorialConsumido;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(interactionSprite != null)
            interactionSprite.gameObject.SetActive(false);

        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged += AtualizarSprite;
    }
    private void OnDestroy()
    {
        if(DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged -= AtualizarSprite;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (apenasUmaVez && tutorialConsumido) return;

        playerInput = other.GetComponent<PlayerInput>();
        jogadorDentro = true;

        AtualizarSprite(DeviceSpriteManager.Instance.GetCurrentDevice());

        if (interactionSprite != null)
            interactionSprite.gameObject.SetActive(true);

        if (interactionIcon != null)
            interactionIcon.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jogadorDentro = false;

        if(interactionSprite != null)
            interactionSprite.gameObject.SetActive(false);

        if(interactionIcon != null)
            interactionIcon.Hide();
    }



    // Update is called once per frame
    void Update()
    {
        if (!jogadorDentro || playerInput == null) return;
        if (TutorialGlobal.Instance.IsTutorialActive) return;

        if (playerInput.actions["Interaction"].WasPerformedThisFrame())
        {
            AbrirTutorial();
        }
    }

    private void AbrirTutorial()
    {
        tutorialConsumido = true;

        if(interactionSprite != null)
            interactionSprite.gameObject.SetActive(false);

        if(interactionIcon != null)
            interactionIcon.Hide();

        TutorialGlobal.Instance.AbrirTutorial(playerInput);
    }

    private void AtualizarSprite(string device)
    {
        if (interactionSprite == null) return;

        interactionSprite.sprite =
            DeviceSpriteManager.Instance.GetSprite(DeviceSpriteManager.InputIconType.Interact);
    }
}
