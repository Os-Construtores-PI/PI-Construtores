using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDIalogos : MonoBehaviour
{
    [Header("UI do Di�logo")]
    public GameObject _dialoguePanel; // painel com fundo
    public TextMeshProUGUI _dialogueText; // texto do  di�logo
    public Button _nextButton; // bot�o opcional (se quiser clicar ao inv�s de apertar espa�o)

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

    public bool _dialogoConcluido { get; private set; } = false;

    // Update is called once per frame

    private void Start()
    {
        _dialoguePanel.SetActive(false);
        _index = 0;
        _dialogoAtivo = true;
        

        if (_nextButton != null)
            _nextButton.onClick.AddListener(NextSentence);
    }
    private void Update()
    {
        if (_dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            NextSentence();
        }
    }

    public void AtivarDialogo()
    {
        Debug.Log("ATIVAR DIALOGO!");
        _dialogoAtivo = true;
        _dialogoConcluido = false;
        _index = 0;

        _dialoguePanel.SetActive(true);

        if (_player != null)
            _player.SetActive(false);

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
        _dialoguePanel.SetActive(false);
        _dialogoAtivo = false;
        _dialogoConcluido = true;

        if (_player != null)
            _player.SetActive(true);
    }
}
