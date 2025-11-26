using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea] public string _dialogo;

    private bool _primeiraVez = true;
    
    
    private void OnTriggerEnter(Collider other)
    {
       if (!other.CompareTag("Player")) return;

       DialogueGlobal.Instance.SetTrigger(this);

       if(_primeiraVez)
       {
          _primeiraVez = false;
          DialogueGlobal.Instance.AbrirDialogo(_dialogo);
       }
    } 

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (DialogueGlobal.Instance != null)
        {
           DialogueGlobal.Instance.SetTrigger(null);
           DialogueGlobal.Instance.FecharDialogo();
        }
    }
}
