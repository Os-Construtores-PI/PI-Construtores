using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogos : MonoBehaviour
{
    [Header("Icone Pulsante")]
    public ImageTriggerEvent _pulseIcon;

    public float _tempoParaRetornarIcone = 2f;

    [Header("UI do Di�logo")]
    public GameObject _dialoguePanel; // painel com fundo
    public TextMeshProUGUI _dialogueText; // texto do  di�logo
    public Button _nextButton; // bot�o opcional (se quiser clicar ao inv�s de apertar espa�o)
    [SerializeField] private CanvasGroup _fadeBG;
    [SerializeField] private CanvasGroup _DialoguePainel;

    [Header("Falas do Tutorial")]
    [TextArea(3, 5)]
    public string[] _sentences; // lista de falas
    private int _index = 0;

    [Header("Config")]
    public float _typingSpeed = 0.03f; // velocidade da escrita
    private Coroutine _typingCoroutine;

    [Header("Controle de Jogo")]
    public GameObject _player; // referencia ao jogador ou controlador do jogo
    private bool _dialogoAtivo = false;
    public bool _podeReabrirDialogo = false;
    public bool _jaMostrouDialogo = false;

    public KeyCode _tecladoDialogo = KeyCode.G;

    public bool _dialogoConcluido { get; private set; } = false;

    // Update is called once per frame

    private void Start()
    {
        _dialoguePanel.SetActive(false);
       // _index = 0;
       // _dialogoAtivo = true;
        

        if (_nextButton != null)
            _nextButton.onClick.AddListener(NextSentence);
    }
    private void Update()
    {
        if (_podeReabrirDialogo && !_dialogoAtivo && Input.GetKeyDown(_tecladoDialogo))
        {
            AtivarDialogo();
           // return;
            
        }

        // ✔ Com o diálogo ativo → G avança
        if (_dialogoAtivo && Input.GetKeyDown(_tecladoDialogo))
        {
            NextSentence();
        }
        
    }

    public void AtivarDialogo()
    {
       
       
       
        _fadeBG.gameObject.SetActive(true);
        _fadeBG.DOFade(1f, 0.3f);



        _dialogoAtivo = true;
        _dialogoConcluido = false;
        _index = 0;

        _dialoguePanel.SetActive(true);

        if (_player != null)
            _player.SetActive(false);
        
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (_pulseIcon != null)
            _pulseIcon.HideIcon();
        _podeReabrirDialogo = false;

        _typingCoroutine = StartCoroutine(TypeSentence(_sentences[_index]));

    }

    IEnumerator TypeSentence(string sentence)
    {
        _dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }
    }

    public void NextSentence()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        if (_dialogueText.text != _sentences[_index])
        {
            // se ainda est� escrevendo -> completa instantaneamente
            _dialogueText.text = _sentences[_index];
        }
        else
        {
            // próxima fala
            _index++;
            if (_index < _sentences.Length)
            {
                _typingCoroutine = StartCoroutine(TypeSentence(_sentences[_index]));

            }
            else
            {
                EncerrarDialogo();
            }
        }
    }
    
    private void EncerrarDialogo()
    {

        _fadeBG.DOFade(0f, 0.3f).OnComplete(() =>
        {
            _fadeBG.gameObject.SetActive(false);
        });
        _dialoguePanel.SetActive(false);
        _dialogoAtivo = false;
        _dialogoConcluido = true;

        if (_player != null)
            _player.SetActive(true);

        if (_pulseIcon != null)
            StartCoroutine(ReturnIcon());
    }

    public void ReiniciarDialogos()
    {
        _dialogoAtivo = false;
        _dialogoConcluido = false;
        _index = 0;

        _dialoguePanel.SetActive(false);

        if(_player != null)
            _player.SetActive(true);
    }

    public void PermitirReabrirDialogo()
    {
        _podeReabrirDialogo = true;
    }
    public void BloquearReabrirDialogo()
    {
        _podeReabrirDialogo = false;
    }

    IEnumerator ReturnIcon()
    {
        yield return new WaitForSeconds(_tempoParaRetornarIcone);

        if (_pulseIcon != null)
            _pulseIcon.ShowIcon();

        _podeReabrirDialogo = true;
    }
    
}
