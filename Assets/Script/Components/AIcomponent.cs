using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Garante que o GameObject tenha um BrainComponent para acessar dados da entidade
[RequireComponent(typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    [SerializeField] private LayerMask layer;              // Camada usada para detectar alvos no raio de visão
    [SerializeField] private string[] tags_methods = { "Spawner", "Weapon", "Hitbox" }; // Tags para encontrar método de dano
    [SerializeField] private float radius;                  // Raio de detecção para encontrar alvos
    [SerializeField] private bool can_AI = true;            // Flag para ativar/desativar o comportamento da IA
    [SerializeField] private float speed = 10;              // Velocidade para movimentação manual
    [SerializeField] private float acceleration = 10;       // Aceleração para movimentação manual

    private BrainComponent brain;                            // Referência ao componente Brain (dados da entidade)
    private CharacterController character;                   // Referência ao CharacterController para movimentação manual
    private Transform target;                                // Alvo atual detectado
    private Transform method_of_damage;                      // Referência a algum filho que representa método de dano (ex: arma)
    private NavMeshAgent automatic;                          // Componente para movimentação automática via NavMesh
    private CharacterController manual;                      // Componente para movimentação manual

    // Inicialização
    private void Awake()
    {
        // Obtém componentes Brain e CharacterController
        TryGetComponent(out brain);
        TryGetComponent(out character);

        // Se existir os componentes, prepara o modo de IA baseado no tipo do inimigo
        if (brain && character)
        {
            PrepareMode(ChooseMode(brain));
        }

        // Encontra método de dano baseado nas tags configuradas
        CheckMethod();
    }

    // Chamado em intervalo fixo, usado para física e movimentação
    void FixedUpdate()
    {
        if (!brain || !character) return;

        // Executa lógica de IA apenas se estiver habilitada e se existir método de dano
        if (can_AI && method_of_damage)
        {
            AI(brain, character, radius);
        }
    }

    // Lógica principal da IA, movimenta em direção ao alvo detectado
    void AI(BrainComponent cabecao, CharacterController controller, float rad)
    {
        // Busca alvo dentro do raio usando visão
        target = VisionAI(rad);

        if (target != null)
        {
            if (automatic != null)
            {
                // Movimenta automaticamente com NavMeshAgent para o destino do alvo
                automatic.SetDestination(target.position);
            }
            else
            {
                // Movimentação manual: calcula direção no plano XZ (ignora Y)
                Vector3 dir = (target.position - transform.position);
                dir = new Vector3(dir.x, 0, dir.z).normalized;

                // Aplica suavização na movimentação com interpolação e aceleração
                Vector3 move_vector = Vector3.Lerp(
                    manual.velocity,
                    speed * Time.deltaTime * dir,
                    1 - Mathf.Exp(-acceleration * Time.deltaTime));

                // Move o personagem manualmente
                manual.Move(move_vector);
            }
        }
    }

    // Busca por um alvo válido dentro do raio de visão
    private Transform VisionAI(float rad)
    {
        // Array fixo para armazenar resultados da sobreposição de esfera
        Collider[] result = new Collider[10];

        // Faz uma checagem física para detectar colisores dentro do raio na camada especificada
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, rad, result, layer);

        for (int i = 0; i < quantity; i++)
        {
            Collider subtarget = result[i];

            // Se o objeto tiver BrainComponent e for do tipo jogador, retorna como alvo
            if (subtarget.TryGetComponent(out BrainComponent brain))
            {
                if (brain.identity.TipoEntidade == EntityType.PLAYER)
                {
                    return subtarget.transform;
                }
            }
        }
        // Se nenhum alvo encontrado, retorna a própria posição (para não mover)
        return transform;
    }

    // Verifica entre os filhos se algum possui tags que indiquem método de dano
    private void CheckMethod()
    {
        foreach (Transform child in transform)
        {
            if (tags_methods.Contains(child.tag))
            {
                method_of_damage = child;
                break;
            }
        }
    }

    // Define o modo de IA baseado no tipo do inimigo definido no BrainComponent
    private AIType ChooseMode(BrainComponent brain)
    {
        return brain.identity.TipoInimigo switch
        {
            EnemyType.SIMPLE => AIType.AUTOMATIC,
            EnemyType.RANGED => AIType.AUTOMATIC,
            EnemyType.TANK => AIType.AUTOMATIC,
            EnemyType.FLYING => AIType.MANUAL,
            _ => AIType.NONE,
        };
    }

    // Prepara o modo de movimentação baseado no tipo da IA (automatic/manual)
    private void PrepareMode(AIType type)
    {
        if (type != AIType.NONE)
        {
            switch (type)
            {
                case AIType.AUTOMATIC:
                    // Adiciona NavMeshAgent para movimentação automática
                    automatic = gameObject.AddComponent<NavMeshAgent>();
                    break;
                case AIType.MANUAL:
                    // Adiciona CharacterController para movimentação manual via código
                    manual = gameObject.AddComponent<CharacterController>();
                    break;
            }
        }
    }
}
