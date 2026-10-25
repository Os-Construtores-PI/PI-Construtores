using System.Collections;
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

  [Header("Textos do Tutorial")]
  [SerializeField]
  private TutorialSpriteText[] _textosTutorial;

  [Header("Indicador do Tutorial")]
  [SerializeField] 
  private ImageTriggerEvent _imagemInteracao;

  [SerializeField]
  private float _tempoOculto = 5f;

  private Coroutine _rotinaIndicador;

  private Player _ultimoPlayer;
  private PlayerInput _ultimoPlayerInput;

  private bool _aguardandoSaidadoTrigger;

  private void Start()
  {

    if (_tutorialGlobal != null)
{
    _tutorialGlobal.OnTutorialStateChanged -= OnTutorialStateChanged;
    _tutorialGlobal.OnTutorialStateChanged += OnTutorialStateChanged;
}

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

    RegisterPlayer(player, other);
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
    {
        Debug.LogWarning(
            "[TutorialTrigger] HUDDirector ou Player não encontrado.",
            this
        );
        return;
    }

    bool success = _hudDirector.TutorialInteractionPopup(
        _player.ID,
        show
    );

    if (success)
        _popupVisible = show;
    else
        Debug.LogWarning(
            $"[TutorialTrigger] Não foi possível " +
            $"{(show ? "mostrar" : "ocultar")} o popup do jogador {_player.ID}.",
            this
        );
  }


  public void OpenTutorial()
  {
    if (!_playerInside || _player == null)
      return;

    OpenTutorialInternal();
  }

  public void OpenTutorialInternal()
  {
    if (!_playerInside || _player == null)
        return;

    if (_tutorialVideo == null)
    {
        Debug.LogWarning(
            $"[TutorialTrigger] Nenhum vídeo configurado em {gameObject.name}.",
            this
        );
        return;
    }

    ResolveReferences();

    if (_tutorialGlobal == null)
    {
        Debug.LogError(
            "[TutorialTrigger] TutorialGlobal não encontrado."
        );
        return;
    }

    if (_tutorialGlobal.IsTutorialActive)
        return;

    ApplyTutorialSprites();

    _tutorialConsumed = true;

    // Esconde a imagem somente quando o tutorial é solicitado.
    OcultarIndicadorTemporariamente();

    _tutorialGlobal.OpenTutorial(_tutorialVideo);
  }

  public void OpenTutorialAutomatically(Player player)
  {
    if (player == null)
        return;

    if (_onlyOnce && _tutorialConsumed)
        return;
    
    if (_player != player)
    {
      Debug.LogWarning(
        "[TutorialTrigger] o Jogador não corresponde ao jogador registrado",
        this
      );

      return;
    }

    OpenTutorialInternal();
  }

  private void OnTriggerExit(Collider other)
  {
    Player exitingPlayer = other.GetComponentInParent<Player>();

    if (exitingPlayer == null || exitingPlayer != _player)
        return;

    _playerCollidersInside.Remove(other);

    if (_playerCollidersInside.Count > 0)
        return;

    SetInteractionPopup(false);

    _player.BlockJumpByTutorialTrigger = false;

    _playerInside = false;
    _playerInput = null;
    _player = null;

    // Só inicia os 5 segundos depois de fechar o tutorial
    // e sair da área.
    if (_aguardandoSaidadoTrigger &&
    (_tutorialGlobal == null || !_tutorialGlobal.IsTutorialActive))
{
    _aguardandoSaidadoTrigger = false;
    OcultarIndicadorTemporariamente();
}
  }

  private void OnEnable()
  {
    ResolveReferences();

    if (_tutorialGlobal != null)
        _tutorialGlobal.OnTutorialStateChanged += OnTutorialStateChanged;
  }

  private void OnTutorialStateChanged(bool isActive)
  {
    if (isActive)
    {
        // Cancela qualquer contagem enquanto o tutorial estiver aberto.
        if (_rotinaIndicador != null)
        {
            StopCoroutine(_rotinaIndicador);
            _rotinaIndicador = null;
        }

        if (_imagemInteracao != null)
            _imagemInteracao.Hide();

        SetInteractionPopup(false);

        _aguardandoSaidadoTrigger = false;
        return;
    }

    // O tutorial foi fechado.
    _aguardandoSaidadoTrigger = true;

    // Se o jogador já saiu, não precisamos esperar outro OnTriggerExit.
    if (!_playerInside)
    {
        _aguardandoSaidadoTrigger = false;
        OcultarIndicadorTemporariamente();
    }
  }



  private void OnDisable()
  {
    if (_rotinaIndicador != null)
    {
        StopCoroutine(_rotinaIndicador);
        _rotinaIndicador = null;
    }

    if (_imagemInteracao != null)
        _imagemInteracao.Hide();

    if (_popupVisible && _hudDirector != null && _player != null)
        _hudDirector.TutorialInteractionPopup(_player.ID, false);

    if (_player != null)
        _player.BlockJumpByTutorialTrigger = false;

    if (_tutorialGlobal != null)
    _tutorialGlobal.OnTutorialStateChanged -= OnTutorialStateChanged;

    _popupVisible = false;
    _playerCollidersInside.Clear();

    _playerInside = false;
    _playerInput = null;
    _player = null;
  }

  public void RegisterPlayer(Player player, Collider playerCollider)
  {
    if (player == null || playerCollider == null)
        return;

    if (_player != null && _player != player)
        return;

    PlayerInput input = player.GetComponent<PlayerInput>();

    if (input == null)
        input = player.GetComponentInParent<PlayerInput>();

    if (input == null)
    {
        Debug.LogWarning(
            "[TutorialTrigger] PlayerInput não encontrado.",
            this
        );
        return;
    }

    bool primeiraEntrada = !_playerInside;

    _player = player;
    _playerInput = input;
    _ultimoPlayer = player;
    _ultimoPlayerInput = input;

    _playerCollidersInside.Add(playerCollider);
    _playerInside = true;

    _player.BlockJumpByTutorialTrigger = true;

    DeviceInputManager.Instance?.ForceRefresh();
    ResolveReferences();

    if (_tutorialGlobal != null && _tutorialGlobal.IsTutorialActive)
    {
        SetInteractionPopup(false);
        return;
    }

    // Se já existe uma rotina, deixa ela terminar.
    if (_rotinaIndicador != null)
        return;

    if (primeiraEntrada)
    {
      if (_imagemInteracao != null)
          _imagemInteracao.Show();

      
      SetInteractionPopup(true);
      return;
    }

    // Se o tutorial já foi consumido, não mostra o popup
    // quando ele só pode ser aberto uma vez.
    if (_onlyOnce && _tutorialConsumed)
    {
        SetInteractionPopup(false);
        return;
    }

    if (_imagemInteracao != null)
        _imagemInteracao.Show();

    SetInteractionPopup(true);
  }

  private void ApplyTutorialSprites()
  {
    ResolveReferences();

    if (_tutorialGlobal == null)
      return;

    if (_textosTutorial == null)
    {
      _tutorialGlobal.SetTutorialSprites(null);
      return;
    }

    Sprite[] sprites = new Sprite[_textosTutorial.Length];

    for (int i = 0;  i < _textosTutorial.Length; i++)
    {
      sprites[i] = _textosTutorial[i].sprite;
    }

    _tutorialGlobal.SetTutorialSprites(sprites);
  }


  private void OcultarIndicadorTemporariamente()
{
    if (_rotinaIndicador != null)
    {
        StopCoroutine(_rotinaIndicador);
        _rotinaIndicador = null;
    }

    _rotinaIndicador = StartCoroutine(RotinaIndicador());
}

private IEnumerator RotinaIndicador()
{
    // Esconde a imagem de interação.
    if (_imagemInteracao != null)
        _imagemInteracao.Hide();

    yield return new WaitForSecondsRealtime(_tempoOculto);

    ResolveReferences();

    // Se o tutorial ainda estiver aberto, não ativa o ícone.
    if (_tutorialGlobal != null &&
        _tutorialGlobal.IsTutorialActive)
    {
        _rotinaIndicador = null;
        yield break;
    }

    // O ícone pode reaparecer mesmo com o jogador fora da área.
    if (_imagemInteracao != null)
        _imagemInteracao.Show();

    _rotinaIndicador = null;
}

  [System.Serializable]
  public class TutorialSpriteText
  {
    public string nome;
    public Sprite sprite;
  }

}
