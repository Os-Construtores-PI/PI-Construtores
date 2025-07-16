using UnityEngine;

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
    private float visionIntervalwalker;



    [HideInInspector] public Vector3 spawnpos;
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
        }
        UpdateAttackLogic();
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
