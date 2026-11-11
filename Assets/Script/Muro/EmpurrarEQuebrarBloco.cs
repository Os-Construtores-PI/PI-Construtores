using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class EmpurrarEQuebrarBloco : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float forcaEmpurrao = 15f;
    [Range(0f, 90f)][SerializeField] private float anguloVariacao = 30f; // Controla a aleatoriedade

    [Header("Configurações de Destruição")]
    [SerializeField] private bool seQuebraAoTocar = false; // Marque True no Inspector apenas nos blocos que devem quebrar
    [SerializeField] private GameObject prefabBlocoFraturado; // Arraste o prefab dos pedaços aqui
    [SerializeField] private float tempoParaDesaparecer = 2f; // Tempo em segundos para sumir
   // [SerializeField] ParticleSystem PedacoBloco;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.Sleep(); // O objeto fica parado até sofrer uma força externa
    }

    void Update()
    {
        //AtivarPedacos();
    }

    private void OnCollisionEnter(Collision colisao)
    {
        if (colisao.gameObject.CompareTag("Player"))
        {
            // 1. Calcula a direção base (frente do jogador)
            Vector3 direcaoBase = transform.position - colisao.transform.position;
            direcaoBase.y = 0; // Mantém o cálculo no plano horizontal
            direcaoBase.Normalize();

            // 2. Adiciona uma variação aleatória baseada no ângulo escolhido
            float aleatoriedadeAngulo = Random.Range(-anguloVariacao, anguloVariacao);
            Quaternion rotacaoAleatoria = Quaternion.Euler(0, aleatoriedadeAngulo, 0);
            Vector3 direcaoFinal = rotacaoAleatoria * direcaoBase;

            // 3. Verifica se este bloco específico deve quebrar
            if (seQuebraAoTocar && prefabBlocoFraturado != null)
            {
                QuebrarObjeto(direcaoFinal);
            }
            else
            {
                EmpurrarObjetoInteiro(direcaoFinal);
            }
        }
    }

    private void EmpurrarObjetoInteiro(Vector3 direcao)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direcao * forcaEmpurrao, ForceMode.Impulse);
        }
    }

    void AtivarPedacos()
    {
        // Procura todos os Rigidbodies nos filhos do objeto
        Rigidbody[] pedacos = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in pedacos)
        {
            rb.isKinematic = false; // Permite que a física afete o pedaço
            rb.useGravity = false;   // Ativa a gravidade
        }
    }

    private void QuebrarObjeto(Vector3 direcao)
    {
        // Instancia o prefab fraturado no mesmo lugar e rotação do bloco original
        GameObject fraturado = Instantiate(prefabBlocoFraturado, transform.position, transform.rotation);

        // Aplica a força em cada um dos pedaços menores do prefab
        Rigidbody[] pedacosRb = fraturado.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rbPedaco in pedacosRb)
        {
            // Adiciona uma pequena variação extra para cada pedaço voar de forma única
            Vector3 forcaPedaco = direcao + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.5f), Random.Range(-0.2f, 0.2f));
            rbPedaco.AddForce(forcaPedaco.normalized * forcaEmpurrao, ForceMode.Impulse);
        }
       //Instantiate(PedacoBloco, transform.position, Quaternion.identity);
        //Instantiate(PedacoBloco, transform.position, Quaternion.identity);

        // Destrói os pedaços após alguns segundos para não pesar no jogo
        Debug.Log("bloco sendo destruido");
        Destroy(fraturado, tempoParaDesaparecer);

        // Destrói o bloco principal original imediatamente
        Destroy(gameObject);
    }
}