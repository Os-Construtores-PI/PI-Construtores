using UnityEngine;
using TMPro;

public class DialogueGlobal : MonoBehaviour
{
    public static DialogueGlobal Instance;

    [Header("UI")]
    public GameObject _painelDialogo;
    public TMP_Text _textoDialogo;


    public DialogueTrigger _currentTrigger;

    private string[] _falasAtuais;
    private int _index = 0;
    private bool _dialogoAtivo = false;
    public int dialogoAtivo = 0;

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
        if (_currentTrigger == null) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!_dialogoAtivo)
            {
                IniciarDialogo(_currentTrigger._dialogo);
            }
            else
            {
                ProximaFala();
            }
        }
    }

    public void Falas(bool value)
    {
        if (value)
        {
            dialogoAtivo++;
        }
        else
        {
            dialogoAtivo--;
        }

        if (dialogoAtivo < 0)
        {
            dialogoAtivo = 0;
        }
        if (dialogoAtivo >= _currentTrigger._dialogo.Length-1)
        {
            dialogoAtivo = _currentTrigger._dialogo.Length-1;
        }
        _textoDialogo.text = _currentTrigger._dialogo[dialogoAtivo];

    }

    public void IniciarDialogo(string[] falas)
    {
        if (falas == null || falas.Length == 0) return;

        _falasAtuais = falas;
        _index = 0;
        _dialogoAtivo = true;

        _painelDialogo.SetActive(true);
        _textoDialogo.text = _falasAtuais[_index];
    }

    void ProximaFala()
    {
        if (!_dialogoAtivo) return;
        
        _index++;
        if (_index >= _falasAtuais.Length)
        {
            FecharDialogo();
            return;
        }
        _textoDialogo.text = _falasAtuais[_index];
    }

    public void FecharDialogo()
    {
        _painelDialogo.SetActive(false);
        _dialogoAtivo = false;
        //_falasAtuais = null;
    }
}
