using TMPro;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    public string[] _dialogo;

    private bool _primeiraVez = true;
    public TextMeshProUGUI _TextoTutor;
    public DialogueGlobal _dialogoGlobal;
    public GameObject _iconInteracao; // icon "Press F"
    private bool _jogadorDentro = false;
    

    private void OnTriggerEnter(Collider other)
    {
       

        

        if (!other.CompareTag("Player")) return;

            _dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();

            if(_dialogoGlobal != null)
            _dialogoGlobal._currentTrigger = this;

            _jogadorDentro = true;

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
        _iconInteracao.SetActive(false);  // 👉 some o ícone

        if(_dialogoGlobal != null)
        {
            if(_dialogoGlobal._currentTrigger == this)
                _dialogoGlobal._currentTrigger = null;
           // _dialogoGlobal.FecharDialogo();
        }

        
      }
    

     private void Start()
    {
        
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();
        
        if (_iconInteracao != null) _iconInteracao.SetActive(false);
    }
    private void Update()
    {
         if (_jogadorDentro && Input.GetKeyDown(KeyCode.F))
         {
            
             AbrirDialogo();
         }
    }

    void AbrirDialogo()
    {
        
         _iconInteracao.SetActive(false); // some enquanto o painel esta aberto
        // _dialogoGlobal.IniciarDialogo(_dialogo);
        

        _dialogoGlobal.SetTrigger(this);
        _dialogoGlobal.IniciarDialogo(_dialogo);
    }

    public void OnDialogoFechado()
    {
        if(_jogadorDentro && _iconInteracao != null)
        {
            _iconInteracao.SetActive(true);
        }
    }   
     
}

