using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DialogueGlobal : MonoBehaviour
{
    public static DialogueGlobal Instance;

    [Header("UI")]
    public GameObject _painelDialogo;
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

    public GameObject _botoesDialogo; // grupo de botões que aparecem no diálogo
    public GameObject _botoesGameplay; // grupo de botões da HUD que somem no diálogo

    public event Action OndialogueStart;
    public event Action OndialogueEnd;

    public UnityEngine.UI.Button _botaoAvancar;
    public UnityEngine.UI.Button _botaoRetornar;

    private PlayerInput _Interactable;

    private bool _openCooldown = false;

    

    void Awake()
    {
        Instance = this;

        if(_painelDialogo != null)
            _painelDialogo.SetActive(false);
       // _painelDialogo.SetActive(false);

        playerDirectoor = FindAnyObjectByType<PlayerDirector>();
        
        _gameDirector = FindAnyObjectByType<GameDirector>();

        if (playerDirectoor != null)
            _playerContext = playerDirectoor.FirstPlayerContext;
        

      //_painelDialogo?.SetActive(false);
        
      
      if (_painelDialogo == null) Debug.LogWarning("[DialogueGlobal] _painelDialogo NÃO atribuído!");
      if (_textoDialogo == null) Debug.LogWarning("[DialogueGlobal] _textoDialogo NÃO atribuído!");

        
    }

    public void SetTrigger(DialogueTrigger trigger)
    {
        _currentTrigger = trigger;
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

        if(_openCooldown) return;
        _openCooldown = true;
        Invoke(nameof(ResetCoolDown), 0.1f);



        if (_currentTrigger != null)
            _Interactable = _currentTrigger._playerInput;

        
        if(_Interactable != null)
        {
            try
            {
                _Interactable.actions["AdvanceDialogue"]?.Reset();
                _Interactable.actions["ReturnDialogue"]?.Reset();
            }
            catch{}
        }

        if (falas == null || falas.Length == 0)
            return;
        _playerContext = null;
        if (_currentTrigger != null && _currentTrigger._playerInput != null)
        {
            var playerGO = _currentTrigger._playerInput.gameObject;
            if (playerGO != null)
            {
                var playerComp = playerGO.GetComponent<Player>();
                if (playerComp != null)
                    _playerContext = playerComp.Context;
            }
        }
        OndialogueStart?.Invoke();

        _falasAtuais = falas;
        _index = 0;
        _dialogoAtivo = true;



        _painelDialogo.SetActive(true);
        _painelDialogo.transform.localScale = Vector3.zero;
        _painelDialogo.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        if (_botoesDialogo != null) _botoesDialogo.SetActive(true);
        if (_botoesGameplay != null) _botoesGameplay.SetActive(false);

        MostrarFala(_falasAtuais[_index]);



        if (_gameDirector != null && _playerContext != null)
            _gameDirector.SetLockPlayer(_playerContext, true);
       
    }
    private void ResetCoolDown()
    {
        _openCooldown = false;
    }

    public void ProximaFala()
    {
        if (!_dialogoAtivo) return;
        
        _index++;
        if (_index >= _falasAtuais.Length)
        {
            FecharDialogo();
            return;
        }
        MostrarFala(_falasAtuais[_index]);
    }

    public void VoltarFala()
    {
        if (!_dialogoAtivo) return;

        _index--;

        if(_index < 0) _index = 0;

        _textoDialogo.text = _falasAtuais[_index];  
    }

    public void FecharDialogo()
    {

        
         OndialogueEnd?.Invoke();
        _dialogoAtivo = false;
        //_falasAtuais = null;


        if (_botoesDialogo != null) _botoesDialogo.SetActive(false);
        if(_botoesGameplay != null) _botoesGameplay.SetActive(true);
        


        if(_currentTrigger != null)
          _currentTrigger.OnDialogoFechado();
        _painelDialogo.transform.DOScale(0f, 0.2f)
    .SetEase(Ease.InBack)
    .OnComplete(() =>
    {
        _painelDialogo.SetActive(false);
    });



        if (_gameDirector != null && _playerContext != null)
            _gameDirector.SetLockPlayer(_playerContext, false);

        _playerContext = null;
    }

    private void Update()
    {
        if (!_dialogoAtivo) return;
        if(_Interactable == null) return;

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
        if (_tweenText != null && _tweenText.IsActive())
            _tweenText.Kill();

        _textoDialogo.maxVisibleCharacters = 0;
        _textoDialogo.text = texto;

        // anima letra por letra
        _tweenText = DOTween.To(
            () => _textoDialogo.maxVisibleCharacters,
            v => _textoDialogo.maxVisibleCharacters = v,
            texto.Length,
            0.35f + (texto.Length * 0.01f) // velocidade automática baseada no tamanho
        );
    }

    


}
