using TMPro;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    public string[] _dialogo;

    private bool _primeiraVez = true;
    public TextMeshProUGUI _TextoTutor;
    public DialogueGlobal _dialogoGlobal;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);

        if (other.CompareTag("Player")) {

            _dialogoGlobal._currentTrigger = gameObject.GetComponent<DialogueTrigger>();

            _TextoTutor.text = _dialogo[0];

            _dialogoGlobal.IniciarDialogo(_dialogo);
        }


     

        /* if (_primeiraVez)
         {
            _primeiraVez = false;
            DialogueGlobal.Instance.IniciarDialogo(_dialogo);
         }
      } 

      private void OnTriggerExit(Collider other)
      {
          if (!other.CompareTag("Player")) return;

         // if (DialogueGlobal.Instance != null)

             DialogueGlobal.Instance.SetTrigger(null);
             DialogueGlobal.Instance.FecharDialogo();

      }*/
    }

     void Start()
    {
        _dialogoGlobal = FindAnyObjectByType<DialogueGlobal>();
    }
} 
