using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuGamePandoraPI : MonoBehaviour
{
    [SerializeField] Transform[] _painelMenu;
    
    [SerializeField] Transform[] _painelConfig;
    [SerializeField] Transform[] _parts;

    [SerializeField] Button[] _botoes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // _painelLayout.DOScale(1, 5);
        StartCoroutine(TimeStart());
        PainelStartOff();
        

        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].localScale = Vector3.zero;
        }
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            _painelConfig[i].localScale = Vector3.zero;
        }
        StartCoroutine(AnimaLogo());
    }



    // Update is called once per frame
    void Update()
    {

    }

    public void CenaGame(string Fase1)
    {
        SceneManager.LoadScene(Fase1);
    }

    public void PainelStartOff()
    {
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            _painelConfig[i].DOScale(0, .25f);
        }
    }

    public void PainelCheck() 
    {
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            _painelMenu[i].DOScale(0, .25f);
        }
    }

    public void PainelStartCheck(bool CheckON)
    {
        if (CheckON == true)
        {
            StartCoroutine(TimeStart());
            

        }
        else
        {
            PainelStartOff();
        }
    }

    public void PainelConfigCheck(bool CheckON)
    {
        if (CheckON == true)
        {
            StartCoroutine(TimeConfig());
        }
        else
        {
            PainelStartOff();
        }
    }

    IEnumerator TimeStart()
    {
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            // _painelMenu[i].localScale = Vector3.zero;

            _painelMenu[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _painelMenu[i].DOScale(1, .25f);
        }


        yield return new WaitForSeconds(0.25f);

        AtivarAnimator();

         
    }

    IEnumerator TimeConfig()
    {
        for (int i = 0; i < _painelConfig.Length; i++)
        {
            // _painelMenu[i].localScale = Vector3.zero;

            _painelConfig[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _painelConfig[i].DOScale(1, .25f);
        }
    }

    IEnumerator AnimaLogo()
    {
        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i].DOLocalJump(new Vector3(0, 0, 0), 100, 5, 3f);
            yield return new WaitForSeconds(3f);

            Image img = _parts[i].GetComponent<Image>();
            yield return new WaitForSeconds(1f);
        }
    }

    private void AtivarAnimator()
    {
        foreach(Button botao in _botoes)
        {
            botao.gameObject.GetComponent<Animator>().enabled = true;
        }
    }
    
}
