using System.Collections;

using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuDirector : MonoBehaviour
{
    [SerializeField] Transform[] _painelMenu; // transform que interage com os botões do menu
    [SerializeField] Transform[] _painelConfig; // transform que chama e interage com o painel de config
    [SerializeField] Transform[] _parts;
    [SerializeField] Transform[] _partsConfig;
    [SerializeField] Transform[] _saveMenuGame; // transform que chama o painel do save

    [SerializeField] Transform[] _pulsarImagem; // Imagens que terão o efeito de pulsar
    


    [SerializeField] Button[] _botoes; // Variavel que chama os botoes animados
    private bool _animadoMenu = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // _painelLayout.DOScale(1, 5);
        StartCoroutine(TimeStart()); // inicia a animação dos painés do menu
        PainelStartOff(); // desativa os paineis de configuração

        if(_pulsarImagem != null)
        {
            foreach(var t in _pulsarImagem)
            {
                if (t != null && t.gameObject.activeInHierarchy)
                    t.localScale = Vector3.one;
            }
        }

        // inicializa todos os paineis do menu com escala zero
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].localScale = Vector3.zero;
        }
        // inicializa todos os paineis de configuração com escala zero
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            _painelConfig[i].localScale = Vector3.zero;
        }
        // inicializa todos os paineis de save com escala zero
        for (int i = 0; i < _saveMenuGame.Length; i++)
            _saveMenuGame[i].localScale = Vector3.zero;

    }



    // Update is called once per frame
    void Update()
    {

    }

    public void CenaGame(string Fase1)
    {
        SceneManager.LoadScene(Fase1); // inicia a cena fase1 como teste
    }

    public void PainelStartOff()
    {

        // desativa todos os paineis de configuração com animação de escala para zero
        for (int i = 0; i < _partsConfig.Length; i++)
        {
            _partsConfig[i].DOScale(0, .25f);
        }
    }

    public void PainelCheck()
    {   // desativa todos os paineis do menu principal com animação de escala para zero
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].DOScale(0, .25f);
        }
    }

    public void PainelMusicPartsCheck()
    {
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            _painelConfig[i].DOScale(0, .25f);
        }
    }
    public void AbrirOpcoes()
    {
        _painelMenu[0].DOScale(0, 0.25f);
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].DOScale(0, 0.25f);
        }
        foreach (Button botao in _botoes)
        {
            botao.transform.DOScale(0, 0.25f);
        }

        StartCoroutine(TimeConfig());
        PararImagemPulsando();
    }

    public void FecharOpcoes()
    {

        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].DOScale(1, 0.25f);
        }
        StartCoroutine(TimeStart());
        foreach (Button botao in _botoes)
        {
            botao.transform.DOScale(1, 0.5f);
        }
        AtivarImagensPulsando();
    }

    public void AbrirPainelVolume()
    {
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            _painelConfig[i].DOScale(0, .25f);

        }
        StartCoroutine(TimeConfigSom());
    }
    public void VoltarParaConfi()
    {
        for (int i = 0; i < _partsConfig.Length; i++)
        {
            _partsConfig[i].DOScale(0, .25f);
        }
    }



    public void PainelStartCheck(bool CheckON)
    {   // ativa ou desativa do menu principal baseado no parametro
        if (CheckON == true)
        {
            StartCoroutine(TimeStart()); // se verdadeiro, inicia dos paineis de animação dos painéis


        }
        else
        {
            //  PainelStartOff(); // se false, desativa os paineis
        }
    }

    public void PainelConfigCheck(bool CheckON)
    {   // ativa ou desativa os paineis de config baseado no parametro
        if (CheckON)
        {
            _painelMenu[0].DOScale(0, 0);
            StartCoroutine(TimeConfig()); // se verdadeiro, inicia dos paineis de config
        }
        else
        {
            //   PainelStartOff(); // se falso, desativa os paineis
        }
    }


    public void AbrirPainelSave()
    {
        // fecha o menu principal
        for (int i = 0; i < _painelMenu.Length; i++)
            _painelMenu[i].DOScale(0, .25f);

        // abre os paineis de save
        for (int i = 0; i < _saveMenuGame.Length; i++)
        {
            _saveMenuGame[i].DOScale(1, .25f);
        }
        PararImagemPulsando();
    }

    public void FecharPainelSave()
    {
        for (int i = 0; i < _saveMenuGame.Length; i++)
            _saveMenuGame[i].DOScale(0, .25f);

        // Reabre o menu principal
        for (int i = 0; i < _painelMenu.Length; i++)
            _painelMenu[i].DOScale(1, .25f);

        AtivarImagensPulsando();

    }



    IEnumerator TimeStart()
    {
        if (_animadoMenu) yield break;
        _animadoMenu = false;
        // animação de entrada dos paineis do menu principal
        for (int i = 0; i < _painelMenu.Length; i++)
        {

            // _painelMenu[i].localScale = Vector3.zero;
            // anima cada painel para escala 1.5 e depois volta para 1
            _painelMenu[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _painelMenu[i].DOScale(1, .25f);
        }


        yield return new WaitForSeconds(0.25f);

        AtivarAnimator(); // ativa animadores dos botões
        AtivarImagensPulsando();
        _animadoMenu = true;


    }

    IEnumerator TimeConfig()
    {
        _animadoMenu = true;
        // animação de entrada dos paineis de configuração 
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            // _painelMenu[i].localScale = Vector3.zero;
            // anima cada painel para a escala 1.5 e depois volta para 1
            _painelConfig[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _painelConfig[i].DOScale(1, .25f);
        }

    }
    IEnumerator TimeConfigSom()
    {

        _animadoMenu = true;
        for (int i = 0; i < _partsConfig.Length; i++)
        {
            _partsConfig[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _partsConfig[i].DOScale(1, .25f);
        }
        //_animadoMenu = true;
    }




    private void AtivarAnimator()
    {
        //ativa o componente animator em todos os botoes
        foreach (Button botao in _botoes)
        {
            botao.gameObject.GetComponent<Animator>().enabled = true;
        }
    }

    public void FecharJogo()
    {
        Application.Quit();
        Debug.Log("Fechando jogo");
    }

    private void AtivarImagensPulsando()
    {
        if (_pulsarImagem == null || _pulsarImagem.Length == 0)
            return;

        foreach (Transform img in _pulsarImagem)
        {
            if (img == null) continue;

            img.localScale = Vector3.one;

            img.DOKill();

            img.DOScale(1.05f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        }
    }

    private void PararImagemPulsando()
    {
        if (_pulsarImagem == null) return;

        foreach( Transform img in _pulsarImagem)
        {
            if (img == null) continue;
            img.DOKill();
            img.localScale = Vector3.one; 
        }
    }

}

