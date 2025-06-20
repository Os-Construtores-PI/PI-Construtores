using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGamePandoraPI : MonoBehaviour
{
    [SerializeField] Transform[] _painelMenu;
    [SerializeField] Transform _painelLayout;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _painelLayout.DOScale(1, 3);
        StartCoroutine(TimeStart());
        
       
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

    IEnumerator TimeStart()
    {
        for (int i = 0; i < _painelMenu.Length; i++)
        {
            // _painelMenu[i].localScale = Vector3.zero;

            _painelMenu[i].DOScale(1.5f, .25f);
            yield return new WaitForSeconds(0.25f);
            _painelMenu[i].DOScale(1, .25f);
        }

         
    }

    
    
}
