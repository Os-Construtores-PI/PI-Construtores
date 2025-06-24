using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Garante que o GameObject tenha um BrainComponent obrigatório
[RequireComponent(typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    [Header("Configurações de Detecção")]
    [SerializeField] private LayerMask layer; // Camada que será checada pelo OverlapSphere (ex: "Player")
    [SerializeField] private float radius;    // Raio de detecção para enxergar o jogador
    [SerializeField] private float attackRange = 2f; // Distância mínima para atacar

    [Header("IA")]
    [SerializeField] private bool can_AI = true;         // Habilita/desabilita IA
    [SerializeField] private float visionInterval = 0.5f; // Intervalo de verificação da visão (por coroutine)

    [Header("Movimentação Manual")]
    [SerializeField] private float speed = 10;
    [SerializeField] private float acceleration = 10;

    [Header("Método de Dano")]
    [SerializeField] private string[] tags_methods = { "Spawner", "Weapon", "Hitbox" };

    // Referências internas
    private BrainComponent brain;
    private CharacterController character;
    private NavMeshAgent automatic;
    private CharacterController manual;
    private Animator animator;

    private Transform target;               // Transform do jogador detectado
    private Transform method_of_damage;     // Referência à arma ou objeto de ataque

    private Collider[] result = new Collider[10];        // Buffer fixo para visão
    private Collider[] attackResult = new Collider[5];   // Buffer fixo para ataque (menor)

    // Inicializa referências
    private void Awake()
    {
        TryGetComponent(out brain);
        TryGetComponent(out character);

        if (brain && character)
        {
            PrepareMode(ChooseMode(brain)); // Decide entre NavMesh ou movimentação manual
        }

        CheckMethod(); // Localiza o filho que representa a arma ou dano
        animator = GetComponentInChildren<Animator>(); // Pega animador (assumindo que está num filho)
    }

    // Inicia verificação de visão periódica
    private void Start()
    {
        StartCoroutine(VisionCheckRoutine());
    }

    // Lógica de movimentação e ataque ocorre no FixedUpdate
    private void FixedUpdate()
    {
        if (!brain || !character) return;

        if (can_AI && method_of_damage)
        {
            AI(brain, character);       // Persegue o jogador
            UpdateAttackLogic();       // Verifica se está no alcance de ataque
        }
    }

    // Movimenta o inimigo em direção ao alvo (via NavMesh ou manual)
    void AI(BrainComponent cabecao, CharacterController controller)
    {
        if (target != null && target != transform)
        {
            if (automatic != null)
            {
                // NavMeshAgent leva até o destino
                automatic.SetDestination(target.position);
            }
            else
            {
                // Movimento manual no plano XZ
                Vector3 dir = (target.position - transform.position);
                dir = new Vector3(dir.x, 0, dir.z).normalized;

                // Suaviza movimentação com interpolação exponencial
                Vector3 move_vector = Vector3.Lerp(
                    manual.velocity,
                    speed * Time.deltaTime * dir,
                    1 - Mathf.Exp(-acceleration * Time.deltaTime));

                manual.Move(move_vector);
            }
        }
    }

    // Coroutine periódica para verificar jogadores próximos
    private IEnumerator VisionCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(visionInterval);
            UpdateTarget(); // Atualiza o alvo detectado
        }
    }

    // Detecta um jogador dentro do raio de visão
    private void UpdateTarget()
    {
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, radius, result, layer);

        for (int i = 0; i < quantity; i++)
        {
            Collider subtarget = result[i];

            // Evita detectar a si mesmo ou seus próprios filhos
            if (subtarget.transform == transform || subtarget.transform.IsChildOf(transform))
                continue;

            if (subtarget.TryGetComponent(out BrainComponent b))
            {
                if (b.identity.TipoEntidade == EntityType.PLAYER)
                {
                    target = subtarget.transform; // Alvo encontrado
                    return;
                }
            }
        }

        target = transform; // Nenhum alvo válido, "desativa" a perseguição
    }

    // Verifica se o jogador está no alcance de ataque
    private void UpdateAttackLogic()
    {
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, attackRange, attackResult, layer);

        for (int i = 0; i < quantity; i++)
        {
            Collider nearby = attackResult[i];

            if (nearby.transform == transform || nearby.transform.IsChildOf(transform))
                continue;

            if (nearby.TryGetComponent(out BrainComponent b))
            {
                if (b.identity.TipoEntidade == EntityType.PLAYER)
                {
                    // Dispara a animação de ataque
                    animator?.SetTrigger("Attack");
                    break;
                }
            }
        }
    }

    // Verifica os filhos do inimigo procurando por armas ou objetos de dano
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

    // Define o tipo de IA com base no tipo de inimigo
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

    // Configura o componente de movimentação baseado no tipo
    private void PrepareMode(AIType type)
    {
        if (type != AIType.NONE)
        {
            switch (type)
            {
                case AIType.AUTOMATIC:
                    automatic = gameObject.AddComponent<NavMeshAgent>();
                    break;
                case AIType.MANUAL:
                    manual = gameObject.AddComponent<CharacterController>();
                    break;
            }
        }
    }
}
