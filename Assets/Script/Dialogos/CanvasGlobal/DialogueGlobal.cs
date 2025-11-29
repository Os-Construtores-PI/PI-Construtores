using UnityEngine;
using TMPro;
using System;

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
    private PlayerDirector playerDirectoor;
    private GameDirector _gameDirector;

    public GameObject _botoesDialogo; // grupo de botões que aparecem no diálogo
    public GameObject _botoesGameplay; // grupo de botões da HUD que somem no diálogo

    public event Action OndialogueStart;
    public event Action OndialogueEnd;

    

    void Awake()
    {
        Instance = this;
        _painelDialogo.SetActive(false);

        playerDirectoor = FindAnyObjectByType<PlayerDirector>();
        _gameDirector = FindAnyObjectByType<GameDirector>();

        
    Instance = this;
    if (_painelDialogo == null) Debug.LogWarning("[DialogueGlobal] _painelDialogo NÃO atribuído!");
    if (_textoDialogo == null) Debug.LogWarning("[DialogueGlobal] _textoDialogo NÃO atribuído!");
    _painelDialogo?.SetActive(false);

        
    }

    public void SetTrigger(DialogueTrigger trigger)
    {
        _currentTrigger = trigger;
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
       
       

        if (dialogoAtivo >= _currentTrigger._dialogo.Length)
        {
            dialogoAtivo = _currentTrigger._dialogo.Length-1;
            FecharDialogo();
            return;
        }

        _textoDialogo.text = _currentTrigger._dialogo[dialogoAtivo];
    }

    public void IniciarDialogo(string[] falas)
    {

        
        if(_currentTrigger == null)
       
        
        if (falas == null || falas.Length == 0) return;

        _falasAtuais = falas;
        _index = 0;
        dialogoAtivo = 0;
        _dialogoAtivo = true;


        
        
        OndialogueStart?.Invoke();

        _painelDialogo.SetActive(true);

        if (_botoesDialogo != null) _botoesDialogo.SetActive(true);
        if (_botoesGameplay != null) _botoesGameplay.SetActive(false);
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


        
         OndialogueEnd?.Invoke();

        if (_botoesDialogo != null) _botoesDialogo.SetActive(false);
        if(_botoesGameplay != null) _botoesGameplay.SetActive(true);

        if(_currentTrigger != null)
          _currentTrigger.OnDialogoFechado();
    }
}
