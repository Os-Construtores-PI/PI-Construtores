using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    [SerializeField] private LayerMask layer;
    [SerializeField] private string[] tags_methods = { "Spawner", "Weapon", "Hitbox" };
    [SerializeField] private float radius;
    [SerializeField] private bool can_AI = true;
    [SerializeField] private float speed;
    private BrainComponent brain;
    private CharacterController character;
    private Transform target;
    private Transform method_of_damage;
    private NavMeshAgent automatic;
    private CharacterController manual;


    private void Awake()
    {
        TryGetComponent(out brain);
        TryGetComponent(out character);
        if (brain && character)
        {
            PrepareMode(ChooseMode(brain));
        }
        CheckMethod();
    }
    void FixedUpdate()
    {
        if (!brain || !character) return;

        if (can_AI && method_of_damage)
        {
            AI(brain, character, radius);
        }
    }
    void AI(BrainComponent cabecao, CharacterController controller, float rad)
    {
        target = VisionAI(rad);
        if (target != null)
        {
            if (automatic != null)
            {
                automatic.SetDestination(target.position);
            }
            else
            {
                Vector3 dir = (target.position - transform.position).normalized;
                manual.Move(dir * speed * Time.deltaTime);
            }
        }
    }
    private Transform VisionAI(float rad)
    {
        Collider[] result = new Collider[10];
        int quantity = Physics.OverlapSphereNonAlloc(transform.position, rad, result, layer);
        for (int i = 0; i < quantity; i++)
        {
            Collider subtarget = result[i];
            if (subtarget.TryGetComponent(out BrainComponent brain))
            {
                if (brain.identity.TipoEntidade == EntityType.PLAYER)
                {
                    return subtarget.transform;
                }
            }
        }
        return transform;
    }
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

