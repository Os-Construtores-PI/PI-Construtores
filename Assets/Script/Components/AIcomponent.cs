using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    [Header("Configurações de Detecção")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private float radius;
    [SerializeField] private float attackRange = 2f;

    [Header("IA")]
    [SerializeField] private bool can_AI = true;
    [SerializeField] private float visionInterval = 0.5f;

    [Header("Movimentação Manual")]
    [SerializeField] private float speed = 10;
    [SerializeField] private float acceleration = 10;

    [Header("Método de Dano")]
    [SerializeField] private string[] tags_methods = { "Spawner", "Weapon", "Hitbox" };

    private BrainComponent brain;
    private CharacterController character;
    private NavMeshAgent automatic;
    private CharacterController manual;
    private Animator animator;

    private Transform target;
    private Transform method_of_damage;

    private Collider[] result = new Collider[10];
    private Collider[] attackResult = new Collider[5];

    // Mapeia EnemyType para AIType usando um dicionário
    private static readonly Dictionary<EnemyType, AIType> enemyToAI = new()
    {
        { EnemyType.SIMPLE, AIType.AUTOMATIC },
        { EnemyType.RANGED, AIType.AUTOMATIC },
        { EnemyType.TANK, AIType.AUTOMATIC },
        { EnemyType.FLYING, AIType.MANUAL }
    };

    // Mapeia AIType para uma ação de preparação
    private readonly Dictionary<AIType, Action> prepareActions;

    public AI_component()
    {
        // Inicializa o dicionário de ações
        prepareActions = new Dictionary<AIType, Action>
        {
            { AIType.AUTOMATIC, () => automatic = gameObject.AddComponent<NavMeshAgent>() },
            { AIType.MANUAL, () => manual = gameObject.AddComponent<CharacterController>() }
        };
    }

    private void Awake()
    {
        TryGetComponent(out brain);
        TryGetComponent(out character);

        if (brain != null && character != null)
        {
            var mode = ChooseMode(brain);
            PrepareMode(mode);
        }

        CheckMethod();
        // animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        StartCoroutine(VisionCheckRoutine());
    }

    private void FixedUpdate()
    {
        if (brain == null || character == null || !can_AI || method_of_damage == null)
            return;

        AI(brain, character);
        UpdateAttackLogic();
    }

    void AI(BrainComponent cabecao, CharacterController controller)
    {
        if (target == null || target == transform)
            return;

        if (automatic != null)
        {
            automatic.SetDestination(target.position);
            return;
        }

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

    private IEnumerator VisionCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(visionInterval);
            UpdateTarget();
        }
    }

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

        target = transform;
    }

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
                animator?.SetTrigger("Attack");
                break;
            }
        }
    }

    private void CheckMethod()
    {
        method_of_damage = transform.Cast<Transform>()
            .FirstOrDefault(child => tags_methods.Contains(child.tag));
    }

    private AIType ChooseMode(BrainComponent brain)
    {
        return enemyToAI.TryGetValue(brain.identity.TipoInimigo, out var aiType) ? aiType : AIType.NONE;
    }

    private void PrepareMode(AIType type)
    {
        if (prepareActions.TryGetValue(type, out var action))
        {
            action.Invoke();
        }
    }
}
