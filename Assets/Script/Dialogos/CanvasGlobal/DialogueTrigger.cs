using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueTrigger : InteractableObject
{
    [TextArea(2, 4)]
    public string[] _dialogo;

    //private bool _primeiraVez = true;
    public TextMeshProUGUI _TextoTutor;
    public DialogueGlobal _dialogoGlobal;
    public Image _iconInteracao; // icon "Press F"
    [HideInInspector] public PlayerInput _CurrentPlayerInput;
    private bool _jogadorDentro = false;


    

    private void OnTriggerEnter(Collider other)
    {
       

        

        if (!other.CompareTag("Player")) return;

            _dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();
            _CurrentPlayerInput = other.GetComponent<PlayerInput>();

            if(_dialogoGlobal != null)
            _dialogoGlobal._currentTrigger = this;

            _jogadorDentro = true;

            if(_TextoTutor != null && _dialogo != null && _dialogo.Length > 0)
            _TextoTutor.text = _dialogo[0];

            if(_iconInteracao != null)
            {
              _iconInteracao.sprite = GetCorrentSprite(_CurrentPlayerInput);
              _iconInteracao.gameObject.SetActive(true);
            }

        _dialogoGlobal.SetTrigger(this);

            

           // _dialogoGlobal.IniciarDialogo(_dialogo);
        
    }



     

        

      private void OnTriggerExit(Collider other)
      {

        
          if (!other.CompareTag("Player")) return;  

        _jogadorDentro = false;

        if (_iconInteracao != null)
            _iconInteracao.gameObject.SetActive(false);
        
        
            if(_dialogoGlobal._currentTrigger == this)
                _dialogoGlobal._currentTrigger = null;
           // _dialogoGlobal.FecharDialogo();
        

        
      }
    

     private void Start()
    {
        
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();
        
        if (_iconInteracao != null) _iconInteracao.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (!_jogadorDentro) return;
        if (_CurrentPlayerInput == null) return;
        if(_dialogoGlobal == null) return;

        if(_dialogoGlobal.IsDialogueActive) return;

        if (_CurrentPlayerInput.actions["Interaction"].WasPerformedThisFrame())
        {
            AbrirDialogo();
        }
        
    }


    void AbrirDialogo()
    {

        // _iconInteracao.SetActive(false); // some enquanto o painel esta aberto
        // _dialogoGlobal.IniciarDialogo(_dialogo);
        if (_iconInteracao != null)
            _iconInteracao.gameObject.SetActive(false);
        

        _dialogoGlobal.SetTrigger(this);
        _dialogoGlobal.IniciarDialogo(_dialogo);
    }

    public void OnDialogoFechado()
    {
        if(_jogadorDentro && _iconInteracao != null)
            _iconInteracao.gameObject.SetActive(true);
        
    }   
     
}

