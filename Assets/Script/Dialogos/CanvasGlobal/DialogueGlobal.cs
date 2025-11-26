using UnityEngine;
using TMPro;

public class DialogueGlobal : MonoBehaviour
{
    public static DialogueGlobal Instance;

    [Header("UI")]
    public GameObject _painelDialogo;
    public TMP_Text _textoDialogo;


    private DialogueTrigger _currentTrigger;

    void Awake()
    {
        Instance = this;
        _painelDialogo.SetActive(false);
    }

    public void SetTrigger(DialogueTrigger trigger)
    {
        _currentTrigger = trigger;
    }

    void Update()
    {
       if (_currentTrigger != null && Input.GetKeyDown(KeyCode.F))
       {
          AbrirDialogo(_currentTrigger._dialogo);
       }
    }

    public void AbrirDialogo(string texto)
    {
         _textoDialogo.text = texto;
         _painelDialogo.SetActive(true);
    }

    public void FecharDialogo()
    {
        _painelDialogo.SetActive(false);
    }
}
