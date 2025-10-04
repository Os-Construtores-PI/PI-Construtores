using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


public class AnimatorPrimeiro : MonoBehaviour
{
    [SerializeField] Button[] _botoes; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Dictionary<Button, Coroutine> animaLoops = new Dictionary<Button, Coroutine>();

    public void AtivarAnimatorLoop()
    {
        foreach(Button botao in _botoes)
        {
            botao.onClick.AddListener(() => ParaAnimacao(botao));

            Coroutine loopCoroutine = StartCoroutine(AnimacaoLoop(botao));
            animaLoops.Add(botao, loopCoroutine);
        }
    }


    private IEnumerator AnimacaoLoop(Button botao)
    {
        AnimatorPrimeiro animator = botao.gameObject.GetComponent<AnimatorPrimeiro>();
        animator.enabled = true;
        while (true)
        {
            yield return null;
        }
    }


    public void ParaAnimacao(Button botao)
    {
        if (animaLoops.TryGetValue(botao, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            botao.gameObject.GetComponent<AnimatorPrimeiro>().enabled = false;

            botao.onClick.RemoveListener(() => ParaAnimacao(botao));
            animaLoops.Remove(botao);

            Debug.Log("Botao pressionada - animação encerrada");

        }
    }

   
}


