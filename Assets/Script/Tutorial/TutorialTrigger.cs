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

  [Header("Collider do Tutorial")]
  [SerializeField]
  private Collider _triggerCollider;

  private bool _triggerDesativadoTemporariamente;

  private bool _ultimoEstadoTutorial;

  private bool _esteTriggerAbriuTutorial;

  private void Start()
  {

    ResolveReferences();

    if (_hudDirector == null)
        Debug.LogError("[TutorialTrigger] HUDDirector não encontrado.", this);

    if (_tutorialGlobal == null)
        Debug.LogError("[TutorialTrigger] TutorialGlobal não encontrado.", this);
  }

  private void Awake()
  {
    ResolveReferences();

    if (_triggerCollider == null)
        _triggerCollider = GetComponent<Collider>();
  }

  private void ResolveReferences()
  {
    if (_hudDirector == null)
        _hudDirector = FindAnyObjectByType<HudDirector>(FindObjectsInactive.Include);

    if (_tutorialGlobal == null)
    {
        _tutorialGlobal = TutorialGlobal.Instance;

        if (_tutorialGlobal == null)
            _tutorialGlobal = FindAnyObjectByType<TutorialGlobal>(FindObjectsInactive.Include);

        InscreverEventoTutorial();
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
    ResolveReferences();

    // Fallback: se o evento não chegou, detectamos a mudança por polling.
    if (_tutorialGlobal != null &&
        _tutorialGlobal.IsTutorialActive != _ultimoEstadoTutorial)
    {
        OnTutorialStateChanged(_tutorialGlobal.IsTutorialActive);
    }

    if (_triggerDesativadoTemporariamente)
    {
      // Mesmo com o BoxCollider desativado, verifica
    // se o jogador já saiu da área de interação.
    if (!_triggerCollider.enabled &&
        _playerInside &&
        PlayerSaiuDoTriggerDesativado())
    {
        LiberarJogadorForaDoTrigger();
    }

    return;
    }

    if (!_playerInside || _playerInput == null || _player == null)
        return;

    if (_onlyOnce && _tutorialConsumed)
        return;

    if (_tutorialGlobal == null)
        return;

    if (_tutorialGlobal.IsTutorialActive || GameContext.IsPaused)
        return;

    if (_player.IgnoreGameplayInputThisFrame)
        return;

    InputAction interaction = _playerInput.actions?.FindAction("Interaction");

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

    _esteTriggerAbriuTutorial = true;


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

    // Libera o pulo assim que o jogador sai da área.
    _player.BlockJumpByTutorialTrigger = false;

    SetInteractionPopup(false);

    _playerInside = false;
    _playerInput = null;
    _player = null;

  }

  private void OnEnable()
  {
    ResolveReferences();
    InscreverEventoTutorial();

    if (_tutorialGlobal != null && _tutorialGlobal.IsTutorialActive)
        OnTutorialStateChanged(true);
  }

  private void InscreverEventoTutorial()
{
    if (_tutorialGlobal == null)
        return;

    _tutorialGlobal.OnTutorialStateChanged -= OnTutorialStateChanged;
    _tutorialGlobal.OnTutorialStateChanged += OnTutorialStateChanged;
}

  private void OnTutorialStateChanged(bool isActive)
  {
    // Só reage se este trigger abriu o tutorial.
    if (!_esteTriggerAbriuTutorial)
        return;

    if (isActive == _ultimoEstadoTutorial)
        return;

    _ultimoEstadoTutorial = isActive;

    if (isActive)
    {
        if (_rotinaIndicador != null)
        {
            StopCoroutine(_rotinaIndicador);
            _rotinaIndicador = null;
        }

        _triggerDesativadoTemporariamente = true;

        if (_triggerCollider != null)
            _triggerCollider.enabled = false;

        AtualizarIndicadorEPopup(false);
        return;
    }

    if (isActiveAndEnabled)
        OcultarIndicadorTemporariamente();
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

    _triggerDesativadoTemporariamente = false;
    _ultimoEstadoTutorial = false;

    if (_triggerCollider != null)
        _triggerCollider.enabled = true;

    _popupVisible = false;
    _playerCollidersInside.Clear();
    _playerInside = false;
    _playerInput = null;
    _player = null;
    _esteTriggerAbriuTutorial = false;
  }

  public void RegisterPlayer(Player player, Collider playerCollider)
  {

    if (_triggerDesativadoTemporariamente)
        return;
    
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

    //bool primeiraEntrada = !_playerInside;

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
    AtualizarIndicadorEPopup(false);
    return;
}

// Enquanto a imagem estiver temporariamente oculta,
// não mostre o popup.
if (_rotinaIndicador != null)
{
    AtualizarIndicadorEPopup(false);
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
     _triggerDesativadoTemporariamente = true;

    if (_triggerCollider != null)
        _triggerCollider.enabled = false;

    AtualizarIndicadorEPopup(false);

    yield return new WaitForSecondsRealtime(_tempoOculto);

    while (_tutorialGlobal != null && _tutorialGlobal.IsTutorialActive)
        yield return null;

    // Popup já está escondido; limpa o estado antes de reativar.
    if (_player != null)
        _player.BlockJumpByTutorialTrigger = false;

    _playerCollidersInside.Clear();
    _playerInside = false;
    _playerInput = null;
    _player = null;
    _popupVisible = false;

    _triggerDesativadoTemporariamente = false;
    _rotinaIndicador = null;

    _esteTriggerAbriuTutorial = false;
    _ultimoEstadoTutorial = false;

    // Ao reativar, a Unity chama OnTriggerEnter de novo se o jogador ainda estiver dentro.
    if (_triggerCollider != null)
        _triggerCollider.enabled = true;

    if (!(_onlyOnce && _tutorialConsumed) && _imagemInteracao != null)
        _imagemInteracao.Show();
}

private bool PlayerSaiuDoTriggerDesativado()
  {
    if (_player == null || _triggerCollider == null)
        return false;

    if (_triggerCollider is not BoxCollider box)
        return false;

    Vector3 posicaoLocal =
        box.transform.InverseTransformPoint(_player.transform.position);

    Vector3 centro = box.center;
    Vector3 metade = box.size * 0.5f;

    return posicaoLocal.x < centro.x - metade.x ||
           posicaoLocal.x > centro.x + metade.x ||
           posicaoLocal.y < centro.y - metade.y ||
           posicaoLocal.y > centro.y + metade.y ||
           posicaoLocal.z < centro.z - metade.z ||
           posicaoLocal.z > centro.z + metade.z;
  }

private void LiberarJogadorForaDoTrigger()
  {
    if (_player != null)
        _player.BlockJumpByTutorialTrigger = false;

    if (_popupVisible && _player != null)
        SetInteractionPopup(false);

    _playerCollidersInside.Clear();
    _playerInside = false;
    _playerInput = null;
    _player = null;
    _popupVisible = false;
  }

private void AtualizarIndicadorEPopup(bool mostrar)
{
    if (_imagemInteracao != null)
    {
        if (mostrar)
            _imagemInteracao.Show();
        else
            _imagemInteracao.Hide();
    }

    SetInteractionPopup(mostrar && _playerInside);
}

  [System.Serializable]
  public class TutorialSpriteText
  {
    public string nome;
    public Sprite sprite;
  }

}
