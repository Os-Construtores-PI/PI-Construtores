using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueGlobal : MonoBehaviour
{
    private enum DialogueState
    {
        Closed,
        Opening,
        Open,
        Closing,
    }

    [Header("Settings")]
    [SerializeField]
    private float _delayAntesdotexto = 0.25f;

    [SerializeField]
    private float _tempoPorLetra = 0.015f;

    [Header("UI Layouts")]
    [SerializeField]
    private GameObject pandoraLayout;

    [SerializeField]
    private GameObject enemyLayout;

    [SerializeField]
    private TMP_Text _textoPandora;

    [SerializeField]
    private TMP_Text _textoEnemy;

    [Header("General UI")]
    public GameObject _painelDialogo;
    public GameObject _botoesDialogo;
    public GameObject _botoesGameplay;
    public Button _botaoAvancar;
    public Button _botaoRetornar;

    public static DialogueGlobal Instance;
    public TMP_Text _textoDialogo; // Referência dinâmica baseada no layout

    private DialogueState _state = DialogueState.Closed;
    private Tween _tweenPainel;
    private Tween _tweenText;
    private string[] _falasAtuais;
    private int _index = 0;
    private bool _dialogoAtivo = false;
    private bool _dialogoPronto = false;
    public bool _bloquearJumpTemporariamente;

    private PlayerDirector playerDirectoor;
    private GameDirector _gameDirector;
    private PlayerContext _playerContext;
    private PlayerContext _lockedPlayer;
    private PlayerInput _Interactable;
    private PlayerInput _defaultPlayerInput;

    public DialogueTrigger _currentTrigger;
    public bool IsDialogueActive => _dialogoAtivo;

    public event Action OndialogueStart;
    public event Action OndialogueEnd;

    private bool _blockAdvanceInput = false;

    private float _nextInputTime = 0f;
    private float _inputDelay = 0.15f;

    private bool _waitingForButtonRelease = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerDirectoor = FindAnyObjectByType<PlayerDirector>();
        _gameDirector = FindAnyObjectByType<GameDirector>();

        if (playerDirectoor != null)
            _playerContext = playerDirectoor.FirstPlayerContext;

        if (_playerContext != null)
            _defaultPlayerInput = _playerContext.PlayerInput;

        if (_painelDialogo != null)
            _painelDialogo.SetActive(false);
    }

    public void SetTrigger(DialogueTrigger trigger)
    {
        _currentTrigger = trigger;
        ApplyLayout(trigger._layoutType);
    }

    public void IniciarDialogo(string[] falas)
    {
        _blockAdvanceInput = true;
        if (_state != DialogueState.Closed || falas == null || falas.Length == 0)
            return;

        _state = DialogueState.Opening;
        _dialogoAtivo = true;
        _dialogoPronto = false;
        Time.timeScale = 0;

        SetupInput(true);

        _waitingForButtonRelease = true;

        _Interactable.actions["AdvanceDialogue"]?.Reset();
        _Interactable.actions["ReturnDialogue"]?.Reset();

        OndialogueStart?.Invoke();
        _falasAtuais = falas;
        _index = 0;
        AtualizarVisibilidadedosBotoes();
        LimparFala();
        _painelDialogo.SetActive(true);
        _painelDialogo.transform.localScale = Vector3.zero;

        if (_botoesGameplay != null)
            _botoesGameplay.SetActive(false);

        if (_botoesDialogo != null)
            _botoesDialogo.SetActive(true);

        _tweenPainel?.Kill();
        _tweenText?.Kill();
        StopAllCoroutines();

        _tweenPainel = _painelDialogo
            .transform.DOScale(1f, 0.30f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _state = DialogueState.Open;
                _dialogoPronto = true;
                _blockAdvanceInput = false;
                StartCoroutine(DelayMostrarFala());
            });

        _lockedPlayer = _playerContext;

        if (_lockedPlayer != null)
            _lockedPlayer.BlockJumpByDialogue = true;

        if (_gameDirector != null && _lockedPlayer != null)
            _gameDirector.SetLockPlayer(_lockedPlayer, true);
    }

    private void SetupInput(bool isDialogue)
    {
        _Interactable = _currentTrigger?._playerInput ?? _defaultPlayerInput;
        if (_Interactable == null)
            return;

        var actions = _Interactable.actions;
        actions.Enable();

        if (isDialogue)
        {
            actions["AdvanceDialogue"]?.Enable();
            actions["ReturnDialogue"]?.Enable();
            actions["Move"]?.Disable();
            actions["Attack"]?.Disable();
            actions["Dash"]?.Disable();
            _Interactable.SwitchCurrentActionMap("Dialogue");
        }
        else
        {
            actions["AdvanceDialogue"]?.Disable();
            actions["ReturnDialogue"]?.Disable();
            actions["Move"]?.Enable();
            actions["Attack"]?.Enable();
            actions["Dash"]?.Enable();

            _Interactable.SwitchCurrentActionMap("Player");
        }
    }

    public void ProximaFala()
    {
        if (!_dialogoAtivo || !_dialogoPronto || _state != DialogueState.Open)
            return;

        if (_index >= _falasAtuais.Length - 1)
        {
            FecharDialogo();
            return;
        }

        _index++;
        AtualizarFala();
    }

    public void VoltarFala()
    {
        if (!_dialogoAtivo || !_dialogoPronto || _state != DialogueState.Open || _index <= 0)
            return;

        _index--;
        AtualizarFala();
    }

    public void FecharDialogo()
    {
        if (_state == DialogueState.Closed || _state == DialogueState.Closing)
            return;

        _state = DialogueState.Closing;
        //StopAllCoroutines();

        _dialogoAtivo = false;
        _dialogoPronto = false;
        AtualizarVisibilidadedosBotoes();

        OndialogueEnd?.Invoke();

        if (_botoesDialogo != null)
            _botoesDialogo.SetActive(false);
        if (_botoesGameplay != null)
            _botoesGameplay.SetActive(true);

        if (_gameDirector != null && _lockedPlayer != null)
            _gameDirector.SetLockPlayer(_lockedPlayer, false);

        if (_lockedPlayer != null)
            _lockedPlayer.BlockJumpByDialogue = false;

        _currentTrigger?.OnDialogoFechado();

        StartCoroutine(FechaDialogoSeguro());
    }

    private void Update()
    {
        if (_state != DialogueState.Open || !_dialogoPronto || _Interactable == null)
            return;

        var advanceAction = _Interactable.actions["AdvanceDialogue"];
        var returnAction = _Interactable.actions["ReturnDialogue"];

        if (_waitingForButtonRelease)
        {
            if (!advanceAction.IsPressed())
                _waitingForButtonRelease = false;

            return;
        }

        if (Time.unscaledTime < _nextInputTime)
            return;

        if (!_blockAdvanceInput && advanceAction.WasPressedThisFrame())
        {
            _nextInputTime = Time.unscaledTime + _inputDelay;
            ProximaFala();
            return;
        }

        if (returnAction.WasPressedThisFrame())
        {
            _nextInputTime = Time.unscaledTime + _inputDelay;
            VoltarFala();
        }
    }

    private void AtualizarFala()
    {
        if (_falasAtuais == null || _index < 0 || _index >= _falasAtuais.Length)
            return;

        //StopAllCoroutines();
        MostrarFala(_falasAtuais[_index]);
        AtualizarVisibilidadedosBotoes();
    }

    private void MostrarFala(string texto)
    {
        LimparFala();
        _textoDialogo.text = texto;
        _textoDialogo.maxVisibleCharacters = 0;
        _textoDialogo.ForceMeshUpdate();

        float duracao = Mathf.Clamp(texto.Length * _tempoPorLetra, 0.10f, 1.0f);

        _tweenText = DOTween
            .To(
                () => _textoDialogo.maxVisibleCharacters,
                v => _textoDialogo.maxVisibleCharacters = v,
                texto.Length,
                duracao
            )
            .SetEase(Ease.Linear)
            .SetUpdate(true);
    }

    private System.Collections.IEnumerator DelayMostrarFala()
    {
        yield return new WaitForSecondsRealtime(_delayAntesdotexto);
        AtualizarFala();
    }

    private void LimparFala()
    {
        _tweenText?.Kill();
        _textoDialogo.text = string.Empty;
        _textoDialogo.maxVisibleCharacters = 0;
        _textoDialogo.ForceMeshUpdate();
    }

    private void ApplyLayout(DialogueTrigger.DialogueLayoutType type)
    {
        pandoraLayout.SetActive(type == DialogueTrigger.DialogueLayoutType.Pandora);
        enemyLayout.SetActive(type == DialogueTrigger.DialogueLayoutType.Enemy);
        _textoDialogo =
            (type == DialogueTrigger.DialogueLayoutType.Pandora) ? _textoPandora : _textoEnemy;
    }

    private void AtualizarVisibilidadedosBotoes()
    {
        if (_botaoRetornar != null)
            _botaoRetornar.gameObject.SetActive(_index > 0);
        if (_botaoAvancar != null)
            _botaoAvancar.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _tweenPainel?.Kill();
        _tweenText?.Kill();
    }

    private IEnumerator FechaDialogoSeguro()
    {
        var dialogueAction = _Interactable.currentActionMap.FindAction("AdvanceDialogue");

        while (dialogueAction != null && dialogueAction.IsPressed())
            yield return null;

        yield return null;

        _Interactable.SwitchCurrentActionMap("Player");

        if (_lockedPlayer != null)
        {
            _lockedPlayer.WaitForJumpRelease = true;
        }

        _painelDialogo
            .transform.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _painelDialogo.SetActive(false);
                _state = DialogueState.Closed;
                Time.timeScale = 1f;
            });
    }
}
