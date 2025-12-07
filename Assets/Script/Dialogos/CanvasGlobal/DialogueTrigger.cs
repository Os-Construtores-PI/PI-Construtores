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
    private Player player;
    


    

    private void OnTriggerEnter(Collider other)
    {
       

        

        if (!other.CompareTag("Player")) return;

            //_dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();
            _CurrentPlayerInput = other.GetComponent<PlayerInput>();

           // if(_dialogoGlobal != null)
            _dialogoGlobal._currentTrigger = this;

            _jogadorDentro = true;

            if(_TextoTutor != null && _dialogo != null && _dialogo.Length > 0)
            _TextoTutor.text = _dialogo[0];

            if(other.TryGetComponent(out Player p))
               player = p;

            if(_iconInteracao != null)
        {
            AtualizarIconeDeInteracao();
            _iconInteracao.gameObject.SetActive(true);
        }

          _dialogoGlobal.SetTrigger(this);
        
        
            
        
        
      

            

           // _dialogoGlobal.IniciarDialogo(_dialogo);
        //DialogueGlobal.Instance.SetTrigger(this);   
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
        
        if (_iconInteracao != null)
           _iconInteracao.gameObject.SetActive(false);
        
        GlobalEventBus.Instance.PLAYERINPUTCHANGED.AddListener(OnControlChanged);
    }

    private void OnControlChanged(string _)
    {
        if (_jogadorDentro)
            AtualizarIconeDeInteracao();
    }
    private void Update()
    {
        if (!_jogadorDentro || _CurrentPlayerInput == null || _dialogoGlobal == null)
            return;

        // Atualiza a sprite enquanto não está em diálogo
        if (!_dialogoGlobal.IsDialogueActive)
            AtualizarIconeDeInteracao();

        if (_dialogoGlobal.IsDialogueActive)
            return;

        if (_CurrentPlayerInput.actions["Interaction"].WasPerformedThisFrame())
            AbrirDialogo();
        
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

    public void AtualizarIconeDeInteracao()
    {
        if (_iconInteracao == null || player == null) return;

        _iconInteracao.sprite = GetCorrentSprite(player);
    }   
     
}

