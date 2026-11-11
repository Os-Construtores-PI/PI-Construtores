using UnityEngine;

public class ArremessarBloco : MonoBehaviour
{
    // Força com que o bloco será arremessado
    [SerializeField] private float forcaArremesso = 10f;

    // Força opcional para jogar o bloco um pouco para cima
    [SerializeField] private float forcaParaCima = 2f;

    private Rigidbody rb;

    void Start()
    {
        // Pega o componente Rigidbody do próprio bloco
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verifica se quem tocou no bloco foi o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Calcula a direção do empurrão (do jogador para o bloco)
            Vector3 direcao = transform.position - collision.transform.position;

            // Ignora a diferença de altura no cálculo se quiser um empurrão horizontal puro
            direcao.y = 0;

            // Normaliza o vetor para manter a força consistente independente da distância
            direcao = direcao.normalized;

            // Adiciona um pouco de força para cima para o bloco "voar" em parábola
            direcao.y = forcaParaCima;

            // Aplica a força de impulso no Rigidbody do bloco
            rb.AddForce(direcao * forcaArremesso, ForceMode.Impulse);
            Destroy(gameObject, 2f); // Destroi o bloco após 2 segundos para evitar acúmulo de objetos
        }
       // Destroy(gameObject, 10f); // Destroi o bloco após 2 segundos para evitar acúmulo de objetos
    }
}