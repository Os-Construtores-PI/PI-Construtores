using UnityEngine;

public class ParedeExplosiva : MonoBehaviour
{
    [Header("Configurações da Explosão")]
    public float forcaExplosao = 300f;
    public float raioExplosao = 5f;
    public float forcaParaCima = 1f;

    private bool jaDestruiu = false;

    // Detecta quando o player encosta no collider "Is Trigger" do objeto pai
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem tocou foi o Player e se a parede ainda não foi destruída
        if (other.CompareTag("Player") && !jaDestruiu)
        {
            jaDestruiu = true;
            ExplodirParede(other.transform.position);
        }
    }

    void ExplodirParede(Vector3 posicaoDoToque)
    {
        // Armazena todos os blocos filhos (os cubos da parede)
        Rigidbody[] blocos = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in blocos)
        {
            // Solta os blocos do objeto pai para que eles possam cair livremente
            rb.transform.SetParent(null);

            // Desativa o modo Kinematic para ativar a gravidade e a física instantaneamente
            rb.isKinematic = false;

            // Aplica a força que faz os blocos voarem pelo ar
            rb.AddExplosionForce(forcaExplosao, posicaoDoToque, raioExplosao, forcaParaCima);

            // Programa o bloco individual para desaparecer completamente em 5 segundos
            Destroy(rb.gameObject, 5f);
        }

        // Destrói o objeto pai que sobrou vazio na cena
        //Destroy(gameObject);
    }
}
