using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDIalogos : MonoBehaviour
{
    [Header("UI do Diálogo")]
    public GameObject _dialoguePanel; // painel com fundo
    public TextMeshProUGUI _dialogueText; // texto do  diálogo
    public Button _nextButton; // botão opcional (se quiser clicar ao invés de apertar espaço)

    [Header("Falas do Tutorial")]
    [TextArea(3, 5)]
    public string[] _sentences; // lista de falas
    private int _index = 0;

    [Header("Config")]
    public float _typingSpeed = 0.03f; // velocidade da escrita
    private Coroutine _typingCoroutine;

    // Update is called once per frame

    private void Start()
    {
        _dialoguePanel.SetActive(true);
        _index = 0;
        StartDialogue();

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

    public void StartDialogue()
    {
        if (_sentences.Length > 0)
        {
            _typingCoroutine = StartCoroutine(TypeSentence(_sentences[_index]));
        }
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
            // se ainda está escrevendo -> completa instantaneamente
            _dialogueText.text = _sentences[_index];
        }
        else
        {
            // próxima fala
            _index++;
            if(_index < _sentences.Length)
            {
                _typingCoroutine = StartCoroutine(TypeSentence(_sentences[_index]));
                
            }
            else
            {
                // acabou o tutorial
                _dialoguePanel.SetActive(false);
            }
        }
    }
}
