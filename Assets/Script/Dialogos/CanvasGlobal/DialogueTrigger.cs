using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    public string[] _dialogo;

    private bool _primeiraVez = true;
    public TextMeshProUGUI _TextoTutor;
    public DialogueGlobal _dialogoGlobal;
    public GameObject _iconInteracao; // icon "Press F"

    private string _botãoInteragir;
    //public TextMeshProUGUI _textoBotão;

    public MapementoInteract _mapeamento;
    public UnityEngine.UI.Image iconImage;

    
    private bool _jogadorDentro = false;

    
    [HideInInspector] public PlayerInput CurrentPlayerInput;
    

    private void OnTriggerEnter(Collider other)
    {
       

        

        if (!other.CompareTag("Player")) return;


            _dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();

            if(_dialogoGlobal != null)
            _dialogoGlobal._currentTrigger = (this);

            _jogadorDentro = true;

            CurrentPlayerInput = other.GetComponent<PlayerInput>();

             
            AtualizarIconInteracao();

             

            if(_TextoTutor != null && _dialogo != null && _dialogo.Length > 0)
            _TextoTutor.text = _dialogo[0];

            if(_iconInteracao != null)
            _iconInteracao.SetActive(true);
           

            

           // _dialogoGlobal.IniciarDialogo(_dialogo);
        
    }



     

        

      private void OnTriggerExit(Collider other)
      {

        
          if (!other.CompareTag("Player")) return;  

        _jogadorDentro = false;
        CurrentPlayerInput = null;
       // _iconInteracao.SetActive(false);  // 👉 some o ícone

       if(_iconInteracao != null)
          _iconInteracao.SetActive(false);

        if(_dialogoGlobal != null && _dialogoGlobal._currentTrigger == this)
        {
            /*if(_dialogoGlobal._currentTrigger == this)
                _dialogoGlobal._currentTrigger = null;
                */
            _dialogoGlobal.SetTrigger(null);
            _dialogoGlobal.FecharDialogo();
        }

        
        if (iconImage != null)
        iconImage.sprite = null;

        
      }
    

     private void Start()
    {
        
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();

        if(_iconInteracao != null)
           _iconInteracao.SetActive(false);
        
       // if (_iconInteracao != null) _iconInteracao.SetActive(false);

        
    }
    private void Update()
    {

        // SE o jogador NÃO está dentro → sai
    if (!_jogadorDentro) return;

    // SE não há input → sai
    if (CurrentPlayerInput == null) return;

    // SE o DialogueGlobal não existe → sai
    if (_dialogoGlobal == null) return;

    // Se NÃO estiver no diálogo e apertou o botão → abre
    if(!_dialogoGlobal.IsDialogueActive &&
       CurrentPlayerInput.actions["Interaction"].WasPerformedThisFrame())
    {
        AbrirDialogo();
    }

         
    }

    void AbrirDialogo()
    {
        
        // _iconInteracao.SetActive(false); // some enquanto o painel esta aberto
        // _dialogoGlobal.IniciarDialogo(_dialogo);
        if(_iconInteracao != null)
           _iconInteracao.SetActive(false);
        

        _dialogoGlobal.SetTrigger(this);
        _dialogoGlobal._playerInputForDialogue = CurrentPlayerInput;
        
        _dialogoGlobal.IniciarDialogo(_dialogo);

    }

    public void OnDialogoFechado()
    {
        if(_jogadorDentro && _iconInteracao != null)
           _iconInteracao.SetActive(true);
        
    }   


    private string DetectarDevice()
{
    if (CurrentPlayerInput == null || CurrentPlayerInput.devices.Count == 0)
        return "Keyboard";

    foreach (var device in CurrentPlayerInput.devices)
    {
        if (device is Gamepad gp)
        {
            if (gp is UnityEngine.InputSystem.DualShock.DualShockGamepad)
                return "DualShock";
            

            return "Xbox"; // genérico p/ outros gamepads
        }

        if (device is Keyboard)
            return "Keyboard";
    }

    return "Keyboard";
}

private void AtualizarIconInteracao()
    {
        string device = DetectarDevice();
        Sprite spriteBotao = _mapeamento.GetIcon(device);

        if (iconImage != null && spriteBotao != null) 
            iconImage.sprite = spriteBotao;
    }
     
}

