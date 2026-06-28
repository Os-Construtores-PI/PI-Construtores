using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AparecerAposTempo : MonoBehaviour
{
    // colocado num gameObject junto da Plataforma Pedra animada, ela aparece 

    // Tempo em segundos antes de aparecer
    public float tempoEspera = 2f;
    public GameObject _gameObject;
    //script01
   // private MeshRenderer meshRenderer;

    void Start()
    {
        // Começa com a plataforma invisível/desativada
       
        _gameObject.SetActive(false);
        

        // Inicia a contagem
       StartCoroutine(AparecerDepoisDoTempo());

        //script01
       // meshRenderer = GetComponent<MeshRenderer>();
       // meshRenderer.enabled = false; // Desliga a renderização visual
       // StartCoroutine(MostrarMesh());
    }

    IEnumerator AparecerDepoisDoTempo()
    {
        // Aguarda os segundos definidos
        yield return new WaitForSeconds(tempoEspera);

        // Ativa a plataforma na cena
        _gameObject.SetActive(true);
        Destroy(_gameObject, 20f);
    }

    

    //script01
    // IEnumerator MostrarMesh()
    // {
    //     yield return new WaitForSeconds(tempoEspera);
    //     meshRenderer.enabled = true; // Liga a renderização visual
    //}
}
