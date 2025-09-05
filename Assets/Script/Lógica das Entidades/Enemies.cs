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
            visionIntervalwalker += Time.deltaTime;
            if (visionIntervalwalker >= visionInterval)
            {
                UpdateTarget();
                visionIntervalwalker = 0f;
            }
            attackIntervalwalker += Time.deltaTime;
            if (attackIntervalwalker >= attackInterval)
            {
                UpdateAttackLogic();
                attackIntervalwalker = 0f;
            }
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

            if (subtarget.TryGetComponent(out Player player))
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
        }
    }
}
