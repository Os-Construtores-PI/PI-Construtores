using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using static TutorialGlobal;

public class TutorialTrigger : MonoBehaviour
{

  [Header("Config")]
  [SerializeField]
  private bool _onlyOnce;

  [Header("Tutorial")]
  [SerializeField]
  private VideoClip _tutorialVideo;

  private PlayerInput _playerInput;

  private TutorialGlobal _tutorialGlobal;

  private Player _player;
  private HudDirector _hudDirector;

  private bool _playerInside;

  private bool _tutorialConsumed;

  private bool _popupVisible;

  private readonly HashSet<Collider> _playerCollidersInside = new();

  private void Start()
  {
    _hudDirector = FindAnyObjectByType<HudDirector>();

    _tutorialGlobal = TutorialGlobal.Instance;


    if (_hudDirector == null)
      Debug.LogError("[TutorialTrigger] HUDDirector não encontrado");

    if (_tutorialGlobal == null)
      Debug.LogError("[TutorialTrigger] TutorialGlobal não encontrado");
    
  }

  private void Awake()
  {
    ResolveReferences();
  }

  private void ResolveReferences()
  {
    if (_hudDirector == null)
      _hudDirector = FindAnyObjectByType<HudDirector>(
          FindObjectsInactive.Include
      );

    if (_tutorialGlobal == null)
    {
      _tutorialGlobal = TutorialGlobal.Instance;

      if (_tutorialGlobal == null)
      {
        _tutorialGlobal = FindAnyObjectByType<TutorialGlobal>(
            FindObjectsInactive.Include
        );
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    Player player = other.GetComponentInParent<Player>();

    if (player == null)
      return;

    if (_onlyOnce && _tutorialConsumed)
      return;

    PlayerInput input = player.GetComponent<PlayerInput>();

    if (input == null)
      input = player.GetComponentInParent<PlayerInput>();

    if (input == null)
    {
      Debug.LogWarning(
          "[TutorialTrigger] PlayerInput não encontrado no jogador."
      );
      return;
    }

    // Ignora colliders de outro jogador.
    if (_player != null && _player != player)
      return;

    _player = player;
    _playerInput = input;

    // Registra cada collider que entrou no trigger.
    _playerCollidersInside.Add(other);

    _playerInside = true;

    DeviceInputManager.Instance?.ForceRefresh();

    ResolveReferences();

    SetInteractionPopup(true);
  }


  private void Update()
  {
    if (!_playerInside || _playerInput == null || _player == null)
      return;

    if (_onlyOnce && _tutorialConsumed)
      return;

    // Não escondemos o popup durante a permanência no trigger.
    // Ele será escondido na saída do trigger.
    ResolveReferences();

    if (_tutorialGlobal == null)
      return;

    if (_tutorialGlobal.IsTutorialActive || GameContext.IsPaused)
      return;

    if (_player.IgnoreGameplayInputThisFrame)
      return;

    InputAction interaction =
        _playerInput.actions?.FindAction("Interaction");

    if (interaction != null && interaction.WasPerformedThisFrame())
      OpenTutorial();
  }

  private void UpdateInteractionPopup(bool show)
  {
    if (_hudDirector == null)
      _hudDirector = FindAnyObjectByType<HudDirector>();

    if (_hudDirector == null || _player == null)
      return;

    if (_popupVisible == show)
      return;

    _hudDirector.TutorialInteractionPopup(_player.ID, show);
    _popupVisible = show;
  }

  private void SetInteractionPopup(bool show)
  {
    ResolveReferences();

    if (_hudDirector == null || _player == null)
      return;

    if (_popupVisible == show)
      return;

    bool success = _hudDirector.TutorialInteractionPopup(
        _player.ID,
        show
    );

    if (success)
      _popupVisible = show;
  }


  public void OpenTutorial()
  {
    if (!_playerInside || _player == null)
      return;

    if (_tutorialVideo == null)
    {
      Debug.LogWarning(
          $"[TutorialTrigger] Nenhum vídeo configurado em {gameObject.name}."
      );
      return;
    }

    ResolveReferences();

    if (_tutorialGlobal == null)
    {
      Debug.LogError(
          "[TutorialTrigger] Não foi encontrado um TutorialGlobal. " +
          "Adiciona o componente à cena e associa-o no Inspector."
      );
      return;
    }

    if (_tutorialGlobal.IsTutorialActive)
      return;

    _tutorialConsumed = true;

    // O popup permanece visível enquanto o jogador estiver no trigger.
    _tutorialGlobal.OpenTutorial(_tutorialVideo);
  }

  private void OnTriggerExit(Collider other)
  {
    if (_player == null)
      return;

    Player exitingPlayer = other.GetComponentInParent<Player>();

    if (exitingPlayer != _player)
      return;

    _playerCollidersInside.Remove(other);

    if (_playerCollidersInside.Count > 0)
      return;

    SetInteractionPopup(false);

    _playerInside = false;
    _playerInput = null;
    _player = null;
  }


  private void OnDisable()
  {
    if (_popupVisible && _hudDirector != null && _player != null)
      _hudDirector.TutorialInteractionPopup(_player.ID, false);

    _popupVisible = false;
    _playerCollidersInside.Clear();

    _playerInside = false;
    _playerInput = null;
    _player = null;
  }

}
