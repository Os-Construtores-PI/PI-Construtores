using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueTrigger : InteractableObject
{
    [TextArea(2, 4)]
    public string[] _dialogo = new string[]
    {
        
    };
    

    //private bool _primeiraVez = true;
    public TextMeshProUGUI _TextoTutor;
    public DialogueGlobal _dialogoGlobal;
    public Image _iconInteracao; // icon "Press F"
    public PlayerInput _playerInput;
    private bool _jogadorDentro = false;

    private bool _canInteractAgain = true;

    private bool _dialogoJaAberto = false;

    [Header("Configuração do Dialogo")]
    [SerializeField] private bool _dialogoApenasUmaVez;

    private bool _dialogoConsumido = false;
    
    
    


    

    private void Start()
    {

        _dialogoGlobal = DialogueGlobal.Instance;
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();

        
        

        if (_iconInteracao != null)
            _iconInteracao.gameObject.SetActive(false);

        if(DeviceSpriteManager.Instance != null)
           DeviceSpriteManager.Instance.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDestroy()
    {
        if(DeviceSpriteManager.Instance != null)
           DeviceSpriteManager.Instance.OnDeviceChanged -= OnDeviceChanged;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_dialogoApenasUmaVez && _dialogoConsumido)
            return;

        _playerInput = other.GetComponent<PlayerInput>();
        

        _jogadorDentro = true;

        // Exibe primeira linha do diálogo no tutor
        if (_TextoTutor != null && _dialogo != null && _dialogo.Length > 0)
            _TextoTutor.text = _dialogo[0];
        
        // Mostra sprite atual do painel de interação
        if (_iconInteracao != null)
        {
            AtualizaSpriteDoIcone();
            _iconInteracao.gameObject.SetActive(true);
        }

        // vincula este trigger ao DialogueGlobal
        if (_dialogoGlobal != null)
            _dialogoGlobal.SetTrigger(this);
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _jogadorDentro = false;

        if (_iconInteracao != null)
            _iconInteracao.gameObject.SetActive(false);

        // remove vínculo do trigger
        if (_dialogoGlobal != null && _dialogoGlobal._currentTrigger == this)
            _dialogoGlobal._currentTrigger = null;
    }


    private void Update()
    {

        if(_dialogoGlobal == null)
        {
            _dialogoGlobal = DialogueGlobal.Instance;
            return;
        }
        
        if (!_jogadorDentro || _playerInput == null || _dialogoGlobal == null)
            return;

        if (_dialogoApenasUmaVez && _dialogoConsumido)
            return;

        // Não deixa interagir enquanto o diálogo está ativo
        if (_dialogoGlobal.IsDialogueActive)
            return;

        if (_canInteractAgain && _playerInput.actions["Interaction"].WasPerformedThisFrame())
            AbrirDialogo();
        
            
    }


    public void AbrirDialogo()
    {
        if (_dialogoJaAberto) return;
        _dialogoJaAberto = true;

        try {_playerInput.actions["Interaction"]?.Reset();} catch { }


        if (_iconInteracao != null)
            _iconInteracao.gameObject.SetActive(false);

        _dialogoGlobal.SetTrigger(this);
        _dialogoGlobal.IniciarDialogo(_dialogo);
    }


    public void OnDialogoFechado()
    {
        _dialogoJaAberto = false;

        if (_dialogoApenasUmaVez)
        {
            _dialogoConsumido = true;

            if (_iconInteracao != null)
                _iconInteracao.gameObject.SetActive(false);

            return;
        }
       
        if (_jogadorDentro && _iconInteracao != null)
            _iconInteracao.gameObject.SetActive(true);
        
        BloquearInteracao();
    }


    public void AtualizaSpriteDoIcone()
    {
        if (_iconInteracao == null)
            return;

        if (DeviceSpriteManager.Instance != null)
            _iconInteracao.sprite = DeviceSpriteManager.Instance.GetCurrentSprite();
    }

    private void OnDeviceChanged(string novoDevice)
    {
        if(_jogadorDentro) 
          AtualizaSpriteDoIcone();
    }   

    public void BloquearInteracao()
    {
        _canInteractAgain = false;
        Invoke(nameof(DesbloquearInteracao), 0.15f);
    }
    public void DesbloquearInteracao()
    {
        _canInteractAgain = true;
    }

    
    
}

