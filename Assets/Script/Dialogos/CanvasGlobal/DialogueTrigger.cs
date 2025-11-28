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
        Debug.Log(other.gameObject.name);

        if (other.CompareTag("Player")) {

            _dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();

            _TextoTutor.text = _dialogo[0];
            _jogadorDentro = true;
            _iconInteracao.SetActive(true);

            _dialogoGlobal.IniciarDialogo(_dialogo);
        }
    }



     

        

      private void OnTriggerExit(Collider other)
      {
          if (!other.CompareTag("Player")) return;

        _jogadorDentro = false;
        _iconInteracao.SetActive(false);  // 👉 some o ícone

        if (_dialogoGlobal != null)
            _dialogoGlobal.FecharDialogo();
      }
    

     private void Start()
    {
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();
        _iconInteracao.SetActive(false);
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
         _dialogoGlobal.IniciarDialogo(_dialogo);
    }   
     
}

