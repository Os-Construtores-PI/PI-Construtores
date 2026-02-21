using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DialogueGlobal : MonoBehaviour
{

    private enum DialogueState
    {
        Closed,
        Opening,
        Open,
        Closing
    }


    private DialogueState _state = DialogueState.Closed;

    private Tween _tweenPainel;
    public static DialogueGlobal Instance;

    [Header("UI")]
    public GameObject _painelDialogo;

    [SerializeField] private TMP_Text _textoPandora;
    [SerializeField] private TMP_Text _textoEnemy;
    public TMP_Text _textoDialogo;
    private Tween _tweenText;


    public DialogueTrigger _currentTrigger;

    private string[] _falasAtuais;
    private int _index = 0;
    private bool _dialogoAtivo = false;
    public bool IsDialogueActive => _dialogoAtivo;
    //public int dialogoAtivo = 0;
    private PlayerDirector playerDirectoor;
    private GameDirector _gameDirector;
    private PlayerContext _playerContext;
    private PlayerContext _lockedPlayer;

    public GameObject _botoesDialogo; // grupo de botões que aparecem no diálogo
    public GameObject _botoesGameplay; // grupo de botões da HUD que somem no diálogo

    public event Action OndialogueStart;
    public event Action OndialogueEnd;

    public UnityEngine.UI.Button _botaoAvancar;
    public UnityEngine.UI.Button _botaoRetornar;

    private PlayerInput _Interactable;
    private PlayerInput _defaultPlayerInput;

    

    [SerializeField] private float _delayAntesdotexto = 0.25f;
    [SerializeField] private float _tempoPorLetra = 0.015f;

    private bool _dialogoPronto = false;

    [SerializeField] private GameObject pandoraLayout;
    [SerializeField] private GameObject enemyLayout;

    

    void Awake()
    {

        if(Instance != null && Instance != this)
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


        if(_painelDialogo != null)
            _painelDialogo.SetActive(false);
        

            
        if (playerDirectoor != null)
            _playerContext = playerDirectoor.FirstPlayerContext;


        
      if (_painelDialogo == null) Debug.LogWarning("[DialogueGlobal] _painelDialogo NÃO atribuído!");
      if (_textoDialogo == null) Debug.LogWarning("[DialogueGlobal] _textoDialogo NÃO atribuído!");

        
    }

        

      
        
      

    public void SetTrigger(DialogueTrigger trigger)
    {
        _currentTrigger = trigger;

        ApplyLayout(trigger._layoutType);
    }

    

    /*public void Falas(bool value)
    {
        if (value)
        {
            dialogoAtivo++;
        }
        else
        {
            dialogoAtivo--;
        }

        if (dialogoAtivo < 0)
        {
            dialogoAtivo = 0;
        }
       
       

        if (dialogoAtivo >= _currentTrigger._dialogo.Length)
        {
            dialogoAtivo = _currentTrigger._dialogo.Length-1;
            FecharDialogo();
            return;
        }

        _textoDialogo.text = _currentTrigger._dialogo[dialogoAtivo];
    }*/

    public void IniciarDialogo(string[] falas)
    {

        if (_state != DialogueState.Closed)
            return;
        
        _state = DialogueState.Opening;
        _dialogoAtivo = true;
        _dialogoPronto = false;

        if (falas == null || falas.Length == 0) return;

        if(_currentTrigger != null && _currentTrigger._playerInput != null)
        {
            _Interactable = _currentTrigger._playerInput;
        }
        else if (_defaultPlayerInput != null)
        {
            _Interactable = _defaultPlayerInput;
        }
        else
        {
            Debug.LogError("[DialogueGlobal] Nenhum PlayerInput disponível para o diálogo!");
            return;
        }

        if(_Interactable != null)
        {
            _Interactable = _playerContext.PlayerInput;
        }

        if(_Interactable != null)
        {

            var actions = _Interactable.actions;

            actions.Enable(); // garante que o asset está ativo

            actions["AdvanceDialogue"]?.Enable();
            actions["ReturnDialogue"]?.Enable();

            // opcional: bloquear ações de gameplay durante diálogo
            actions["Move"]?.Disable();
            actions["Attack"]?.Disable();
            actions["Dash"]?.Disable();
        }

        OndialogueStart?.Invoke();
        
        _falasAtuais = falas;
        _index = 0;
        
        LimparFala();
        
        _painelDialogo.SetActive(true);
        _painelDialogo.transform.localScale = Vector3.zero;
        
        if (_botoesDialogo != null) _botoesDialogo.SetActive(true);
        if (_botoesGameplay != null) _botoesGameplay.SetActive(false);

        _tweenPainel?.Kill(true);
        _tweenText?.Kill(true);
        StopAllCoroutines();
       
        _tweenPainel = _painelDialogo.transform
            .DOScale(1f, 0.30f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (_state != DialogueState.Opening)
                    return;

                _state = DialogueState.Open;
                _dialogoPronto = true;

                StartCoroutine(DelayMostrarFala());
            });


        _lockedPlayer = _playerContext;
        
        if (_gameDirector != null && _lockedPlayer != null)
            _gameDirector.SetLockPlayer(_lockedPlayer, true);
        
       



       // _painelDialogo.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

       
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
        if (!_dialogoAtivo || !_dialogoPronto || _state != DialogueState.Open) 
            return;

        if (_index <= 0)
            return;

        _index--;


        AtualizarFala();
       // _textoDialogo.text = _falasAtuais[_index];  
    }

    public void FecharDialogo()
    {
        if (_Interactable != null)
        {
            var actions = _Interactable.actions;

            actions["AdvanceDialogue"]?.Disable();
            actions["ReturnDialogue"]?.Disable();

            // reativa gameplay
            actions["Move"]?.Enable();
            actions["Attack"]?.Enable();
            actions["Dash"]?.Enable();
        }   

        if (_state == DialogueState.Closed || _state == DialogueState.Closing)
            return;

        _state = DialogueState.Closing;
        
        _dialogoAtivo = false;
        _dialogoPronto = false;

        StopAllCoroutines();

        _dialogoAtivo = false;

        //_falasAtuais = null;
        _tweenPainel?.Kill(true);
        _tweenPainel = null;

        OndialogueEnd?.Invoke();
       
        if (_botoesDialogo != null) _botoesDialogo.SetActive(false);
        if(_botoesGameplay != null) _botoesGameplay.SetActive(true);
        
        if (_gameDirector != null && _lockedPlayer != null)
            _gameDirector.SetLockPlayer(_lockedPlayer, false);
        
        _lockedPlayer = null;
        
        
        if(_tweenPainel != null)
        {
            _tweenPainel.Kill();
            _tweenPainel = null;
        }

        


        if(_currentTrigger != null)
          _currentTrigger.OnDialogoFechado();
        
        _painelDialogo.transform.DOScale(0f, 0.2f)
    .SetEase(Ease.InBack)
    .OnComplete(() =>
    {
        _painelDialogo.SetActive(false);
        _state = DialogueState.Closed;
    });



    }

    private void Update()
    {
        if (_Interactable == null)
        {
            Debug.LogWarning("[DialogueGlobal] Interactable NULL – diálogo sem PlayerInput");
            return;
        }

        if (_state != DialogueState.Open)
            return;

        if(_Interactable == null) 
            return;
        

        if (_Interactable.actions["AdvanceDialogue"].WasPerformedThisFrame())
    {
        

        if(_index >= _falasAtuais.Length - 1)
            {
                FecharDialogo();
                return;
            }
        ProximaFala();
    }

    // VOLTAR
    if (_Interactable.actions["ReturnDialogue"].WasPerformedThisFrame())
    {
        

        VoltarFala();   // SEMPRE retorna a fala
    }
    }



    private void MostrarFala(string texto)
    {
        LimparFala();
        _textoDialogo.text = texto;
        _textoDialogo.maxVisibleCharacters = 0;
        _textoDialogo.ForceMeshUpdate();

        float duracao = texto.Length * _tempoPorLetra;
        duracao = Mathf.Clamp(duracao, 0.10f, 1.0f);

        _tweenText = DOTween.To(
            () => _textoDialogo.maxVisibleCharacters,
            v => _textoDialogo.maxVisibleCharacters = v,
            texto.Length,
            duracao
        ).SetEase(Ease.Linear);
    }

    private System.Collections.IEnumerator DelayMostrarFala()
    {
        yield return new WaitForSeconds(_delayAntesdotexto);
        AtualizarFala();
    }


    private void AtualizarFala()
    {
        if (_falasAtuais == null || _falasAtuais.Length == 0)
            return;
        if (_index < 0 || _index >= _falasAtuais.Length)
        {
            Debug.LogWarning($"[DialogueGlobal] Índice inválido: {_index}");
            return;
        }

        StopAllCoroutines();
        MostrarFala(_falasAtuais[_index]);
    }

    private void LimparFala()
    {
        _tweenText?.Kill();
        _tweenText = null;
        _textoDialogo.text = string.Empty;
        _textoDialogo.maxVisibleCharacters = 0;

        // forçar TMP a atualizar imediatamente
        _textoDialogo.ForceMeshUpdate();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _tweenPainel?.Kill();
        _tweenText?.Kill();
    }

    private void ApplyLayout(DialogueTrigger.DialogueLayoutType type)
    {
        pandoraLayout.SetActive(false);
        enemyLayout.SetActive(false);

        switch (type)
        {
            case DialogueTrigger.DialogueLayoutType.Pandora:
                 pandoraLayout.SetActive(true);
                 _textoDialogo = _textoPandora;
                 break;
            
            case DialogueTrigger.DialogueLayoutType.Enemy:
                 enemyLayout.SetActive(true);
                 _textoDialogo = _textoEnemy;
                 break;
        }
    }

}
