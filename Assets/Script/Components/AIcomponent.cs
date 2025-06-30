using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Garante que o componente BrainComponent esteja presente no GameObject
[RequireComponent(typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    // ==== CONFIGURAÇÕES DE DETECÇÃO ====
    [Header("Configurações de Detecção")]
    [SerializeField] private LayerMask layer;        // Camada usada para detectar alvos (ex: jogadores)
    [SerializeField] private float radius;           // Raio da detecção de visão
    [SerializeField] private float attackRange = 2f; // Raio da detecção de ataque

    // ==== COMPORTAMENTO DE IA ====
    [Header("IA")]
    [SerializeField] private bool can_AI = true;         // Permite ativar/desativar IA
    [SerializeField] private float visionInterval = 0.5f; // Intervalo para verificar visão

    // ==== MOVIMENTAÇÃO MANUAL ====
    [Header("Movimentação Manual")]
    [SerializeField] private float speed = 10;
    [SerializeField] private float acceleration = 10;

    // ==== MÉTODO DE DANO ====
    [Header("Método de Dano")]
    [SerializeField] private string[] tags_methods = { "Spawner", "Weapon", "Hitbox" }; // Tags para identificar o método de ataque

    // ==== COMPONENTES INTERNOS ====
    private BrainComponent brain;              // Referência ao "cérebro" da IA
    private CharacterController character;     // Para detectar colisões e movimentar no modo manual
    private NavMeshAgent automatic;            // Para movimentação automática via NavMesh
    private CharacterController manual;        // Referência duplicada para movimentação manual (redundante aqui)
    private Animator animator;                 // Referência opcional para animações

    // ==== ALVOS ====
    private Transform target;                  // Alvo atual da IA
    private Transform method_of_damage;        // Parte do corpo que causará dano (arma, etc.)

    // Buffers para detecção de alvos
    private Collider[] result = new Collider[10];
    private Collider[] attackResult = new Collider[5];

    // Mapeia o tipo de inimigo para o tipo de IA (Ex: TANK => AUTOMATIC)
    private static readonly Dictionary<EnemyType, AIType> enemyToAI = new()
    {
        { EnemyType.SIMPLE, AIType.AUTOMATIC },
        { EnemyType.RANGED, AIType.AUTOMATIC },
        { EnemyType.TANK, AIType.AUTOMATIC },
        { EnemyType.FLYING, AIType.MANUAL }
    };

    // Dicionário que define qual método preparar com base no AIType
    private readonly Dictionary<AIType, Action> prepareActions;

    // Construtor para inicializar o dicionário prepareActions
    public AI_component()
    {
        prepareActions = new Dictionary<AIType, Action>
        {
            { AIType.AUTOMATIC, () => automatic = gameObject.AddComponent<NavMeshAgent>() },
            { AIType.MANUAL, () => manual = gameObject.AddComponent<CharacterController>() }
        };
    }

    private void Awake()
    {
        // Coleta os componentes necessários
        TryGetComponent(out brain);
        TryGetComponent(out character);

        // Determina e prepara o modo de IA (automático ou manual)
        if (brain != null && character != null)
        {
            var mode = ChooseMode(brain);
            PrepareMode(mode);
        }

        // Identifica onde está o método de causar dano
        CheckMethod();

        // (Opcional) Carrega animações se existir
        // animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Começa rotina de verificação de visão
        StartCoroutine(VisionCheckRoutine());
    }

    private void FixedUpdate()
    {
        // Verificações de segurança
        if (brain == null || character == null || !can_AI || method_of_damage == null)
            return;

        // Comportamento da IA
        AI(brain, character);

        // Verifica se pode atacar
        UpdateAttackLogic();
    }

    // Lógica principal da IA
    void AI(BrainComponent cabecao, CharacterController controller)
    {
        if (target == null || target == transform)
            return;

        // Movimento automático via NavMesh
        if (automatic != null)
        {
            automatic.SetDestination(target.position);
            return;
        }

        // Movimento manual via CharacterController
        if (manual == null) 
            return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        Vector3 move_vector = Vector3.Lerp(
            manual.velocity,
            speed * Time.deltaTime * dir,
            1 - Mathf.Exp(-acceleration * Time.deltaTime));

        manual.Move(move_vector);
    }

    // Rotina periódica que checa se há alvos na área de visão
    private IEnumerator VisionCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(visionInterval);
            UpdateTarget();
        }
    }

    // Atualiza o alvo com base na visão
    private void UpdateTarget()
    {
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, radius, result, layer);

        for (int i = 0; i < quantity; i++)
        {
            var subtarget = result[i].transform;

            if (subtarget == transform || subtarget.IsChildOf(transform))
                continue;

            if (subtarget.TryGetComponent(out BrainComponent b) && b.identity.TipoEntidade == EntityType.PLAYER)
            {
                target = subtarget;
                return;
            }
        }

        // Se não encontrar alvo, redefine o alvo para si mesmo
        target = transform;
    }

    // Verifica se há algum alvo próximo o suficiente para ataque
    private void UpdateAttackLogic()
    {
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, attackRange, attackResult, layer);

        for (int i = 0; i < quantity; i++)
        {
            var nearby = attackResult[i].transform;

            if (nearby == transform || nearby.IsChildOf(transform))
                continue;

            if (nearby.TryGetComponent(out BrainComponent b) && b.identity.TipoEntidade == EntityType.PLAYER)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
                break;
            }
        }
    }

    // Verifica qual objeto será usado para causar dano (com base nas tags)
    private void CheckMethod()
    {
        method_of_damage = transform.Cast<Transform>()
            .FirstOrDefault(child => tags_methods.Contains(child.tag));
    }

    // Decide o modo de IA com base no tipo de inimigo
    private AIType ChooseMode(BrainComponent brain)
    {
        return enemyToAI.TryGetValue(brain.identity.TipoInimigo, out var aiType) ? aiType : AIType.NONE;
    }

    // Executa a preparação do modo de IA (cria NavMeshAgent ou CharacterController)
    private void PrepareMode(AIType type)
    {
        if (prepareActions.TryGetValue(type, out var action))
        {
            action.Invoke();
        }
    }
}
