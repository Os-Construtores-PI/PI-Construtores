using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Enemies : CombatEntities
{
    protected Transform target;
    private Collider[] result = new Collider[10];
    private Collider[] attackResult = new Collider[5];
    // ==== CONFIGURAÇÕES DE DETECÇÃO ====
    [Header("Configurações de Detecção")]
    [SerializeField] private LayerMask layer;        // Camada usada para detectar alvos (ex: jogadores)
    [SerializeField, Min(10)] private float radius;           // Raio da detecção de visão
    [SerializeField] private float attackRange = 2f; // Raio da detecção de ataque


    [SerializeField] private float memoryCooldown = 3f;
    [SerializeField] private float memoryCooldownWalker = 0.0f;
    private bool memoryTriggered = false;
    private bool playerInArea = false;
    // ==== COMPORTAMENTO DE IA ====
    [Header("IA")]
    [SerializeField] private bool can_AI = true;         // Permite ativar/desativar IA
    [SerializeField] private float visionInterval = 0.5f; // Intervalo para verificar visão
    [SerializeField] private float attackInterval = 1f;
    private float visionIntervalwalker = 0.0f;
    private float attackIntervalwalker = 0.0f;

    // ==== CONFIGURAÇÂO DE LOOTTABLE ==== //
    protected WeightedTable<string> lootTable = new();
    protected SerializedDictionary<string, float> items = new() {{"item bom",10},{"item ruim",90}};
    // ==== Referência para o Scanner ==== //
    [HideInInspector] public Vector3 spawnpos;

    [Header("Enemy Damage Logic")]
    [SerializeField] private float _dashBlockDuration;
    [SerializeField] private float _knockbackForce = 40f;
    public float KnockBackForce => _knockbackForce;
    public float DashBlockDuration => _dashBlockDuration;


    public override void Start()
    {
        base.Start();
        AddItems();
    }

    public void ApplyKnockBack(Transform player)
    {
        if (player.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 direction = (player.position - transform.position).normalized;
            rb.AddForce(direction * _knockbackForce, ForceMode.Impulse);
        }
    }

    public override void DeathHandler()
    {
        print(lootTable.PickEntry());
    }
    private void AddItems()
    {
        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                lootTable.AddEntry(item.Key, item.Value);
            }
        }
    }
    public override void Update()
    {
        base.Update();
        if (can_AI)
        {
            VisionTimer();
            AttackTimer();
            MemoryTimer();
            
        }

    }
    private void VisionTimer()
    {
            visionIntervalwalker += Time.deltaTime;
            if (visionIntervalwalker >= visionInterval)
            {
                UpdateTarget();
                visionIntervalwalker = 0f;
            }
    }
    private void AttackTimer()
    {
            attackIntervalwalker += Time.deltaTime;
            if (attackIntervalwalker >= attackInterval)
            {
                UpdateAttackLogic();
                attackIntervalwalker = 0f;
            }
    }
    private void MemoryTimer()
    {
        if (!playerInArea && !memoryTriggered) // só executa se o player saiu e ainda não rodou
        {
            memoryCooldownWalker += Time.deltaTime;

            if (memoryCooldownWalker >= memoryCooldown)
            {
                target = transform;
                memoryCooldownWalker = 0.0f;
                memoryTriggered = true; // marca que já rodou
            }
        }
        else if (playerInArea)
        {
            // se o player voltar, reseta o estado
            memoryCooldownWalker = 0.0f;
            memoryTriggered = false;
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
            if (subtarget.TryGetComponent(out Player _))
            {
                playerInArea = true;
                memoryCooldownWalker = .0f;
                target = subtarget;
                return;
            }
        }

        // Se não encontrar alvo, redefine o alvo para si mesmo
        playerInArea = false;
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
           
            
        }

        
    }
}
