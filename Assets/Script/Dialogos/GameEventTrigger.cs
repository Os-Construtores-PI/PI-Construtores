using UnityEngine;
using UnityEngine.Events;

public class GameEventTrigger : MonoBehaviour
{

    public bool _triggerOnce = true; // se true, so dispara uma vez
    public string _playerTag = "Player"; // quem pode ativar

    public UnityEvent onTriggerEnter; // lista de ações ao entrar
    public UnityEvent onTriggerExit; //opcional; ao sair

    private bool _actived = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Reset()
    {
        // garante que o collider esteja como "isTrigger"
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        if (!_actived || !_triggerOnce)
        {
            onTriggerEnter?.Invoke();
            _actived = true;
        }
    }

    /*public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        onTriggerExit?.Invoke();
        _actived = false;
    }
    
    public void Debug_ReabirDialogo()
    {
        FindAnyObjectByType<TutorialDIalogos>().AtivarDialogo();
    }*/
}
