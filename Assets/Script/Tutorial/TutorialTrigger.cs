using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using static TutorialGlobal;

public class TutorialTrigger : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private ImageTriggerEvent _interactionIcon;

  [SerializeField]
  private Image _interactionSprite;

  [Header("Config")]
  [SerializeField]
  private bool _onlyOnce;

  [Header("Tutorial")]
  [SerializeField]
  private VideoClip _tutorialVideo;

  private PlayerInput _playerInput;

  private bool _playerInside;

  private bool _tutorialConsumed;

  private void Start()
  {
    if (_interactionSprite != null)
            _interactionSprite.gameObject.SetActive(false);
  }

  private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_onlyOnce && _tutorialConsumed)
            return;

        _playerInput =
            other.GetComponent<PlayerInput>();

        _playerInside = true;

        DeviceInputManager.Instance?.ForceRefresh();

        if (_interactionSprite != null)
            _interactionSprite.gameObject.SetActive(true);

        if (_interactionIcon != null)
            _interactionIcon.Hide();
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInside = false;

        if (_interactionSprite != null)
            _interactionSprite.gameObject.SetActive(false);

        if (_interactionIcon != null)
            _interactionIcon.Show();
    }


    private void Update()
    {
        if (!_playerInside)
            return;

        if (_playerInput == null)
            return;

        if (TutorialGlobal.Instance == null)
            return;

        if (TutorialGlobal.Instance.IsTutorialActive)
            return;

        if (GameContext.IsPaused)
            return;


        Player player =
            _playerInput.GetComponent<Player>();

        if (
            player != null &&
            player.IgnoreGameplayInputThisFrame
        )
        {
            return;
        }


        if (
            _playerInput.actions["Interaction"]
            .WasPerformedThisFrame()
        )
        {
            OpenTutorial();
        }
    }


    private void OpenTutorial()
    {
        if (_tutorialVideo == null)
        {
            Debug.LogWarning(
                $"[TutorialTrigger] " +
                $"Nenhum vídeo configurado em {gameObject.name}."
            );

            return;
        }


        if (TutorialGlobal.Instance == null)
            return;


        _tutorialConsumed = true;


        if (_interactionSprite != null)
            _interactionSprite.gameObject.SetActive(false);

        if (_interactionIcon != null)
            _interactionIcon.Hide();


        TutorialGlobal.Instance.OpenTutorial(
            _tutorialVideo
        );
    }


    private void OnDisable()
    {
        _playerInside = false;
    }

}
