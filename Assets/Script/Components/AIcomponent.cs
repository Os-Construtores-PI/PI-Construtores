using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(BrainComponent))]
public class AI_component : ComponentBehaviour
{
    private BrainComponent brain;
    private CharacterController character;
    [SerializeField] private float radius;
    private bool can_AI;
    private Transform target;
    [SerializeField] private LayerMask layer;
    [SerializeField] private string[] tags_methods = {"Spawner","Weapon","Hitbox"};
    private Transform method_of_damage;

    private void Awake()
    {
        TryGetComponent(out brain);
        TryGetComponent(out character);
        CheckMethod();
    }
    void FixedUpdate()
    {
        if (!brain || !character) return;

        can_AI = brain.comportamento == BrainComponent.Behavior.AGRESSIVE;

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
                if (brain.identity.TipoEntidade == EntityType.PLAYER) return subtarget.transform;
            }
        }
        return null;
    }
    private void CheckMethod()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (Array.Exists(tags_methods, tag => child.CompareTag(tag)))
            {
                method_of_damage = child;
            }
        }
    }
}
