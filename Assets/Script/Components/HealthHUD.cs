using UnityEngine;
using UnityEngine.UI;

// Componente que controla o HUD de vida (Health Bar) de uma entidade (jogador ou inimigo)
public class HealthHUDComponent : ComponentBehaviour
{
    // Objeto visual que representa a barra de vida
    [SerializeField] private GameObject healthBarObject;

    // Dados de ícone (não está sendo utilizado ativamente neste script)
    [SerializeField] private IconData iconData;

    // Tipo de HUD: PLAYER ou ENEMY
    [SerializeField] public HealthHUDType HUDType;

    // Transform do inimigo que este HUD deve seguir (caso ENEMY)
    public Transform enemy_target;

    // ID único da entidade associada a este HUD
    public int id_health = 0;

    // Referência para o componente Slider que mostra visualmente a vida
    private Slider slider;

    private void Start()
    {
        // Obtém o componente Slider neste GameObject
        if (TryGetComponent(out Slider sl))
        {
            slider = sl;

            // Configuração do HUD com base no tipo
            switch (HUDType)
            {
                case HealthHUDType.PLAYER:
                    // Procura todos os objetos com a tag "Player" na cena
                    var players = GameObject.FindGameObjectsWithTag("Player");

                    foreach (var p in players)
                    {
                        // Verifica se o objeto possui BrainComponent e HealthComponent com ID correspondente
                        if (AlignBrain_ID_ENTITY(p, EntityType.PLAYER, out BrainComponent brain, out HealthComponent health))
                        {
                            // Se encontrar, atualiza o valor do slider com a porcentagem de vida
                            if (health.TryGetAttribute("MAX_health", out float max_Health) &&
                                health.TryGetAttribute("health", out float health_v))
                                slider.value = health_v / max_Health;
                        }
                    }
                    break;

                case HealthHUDType.ENEMY:
                    // Se o alvo do inimigo estiver definido
                    if (enemy_target)
                    {
                        // Assume que o pai do alvo é o GameObject do inimigo
                        GameObject enemy = enemy_target.parent.gameObject;

                        // Verifica se é o inimigo correspondente
                        if (AlignBrain_ID_ENTITY(enemy, EntityType.ENEMY, out BrainComponent brain, out HealthComponent health))
                        {
                            // Atualiza o slider com a vida atual
                            if (health.TryGetAttribute("MAX_health", out float max_Health) &&
                                health.TryGetAttribute("health", out float health_v))
                                slider.value = health_v / max_Health;
                        }
                    }
                    break;
            }
        }
    }

    void LateUpdate()
    {
        // Atualizações específicas para HUDs de inimigos
        if (HUDType == HealthHUDType.ENEMY && enemy_target)
        {
            // Move o HUD para seguir a posição do alvo do inimigo
            transform.position = enemy_target.position;

            // Procura a câmera mais próxima do HUD
            Camera[] cameras = Camera.allCameras;
            Camera closestCam = null;
            float closestDistSqr = Mathf.Infinity;
            Vector3 myPosition = transform.position;

            foreach (Camera cam in cameras)
            {
                float distSqr = (cam.transform.position - myPosition).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestCam = cam;
                    closestDistSqr = distSqr;
                }
            }

            // Gira o HUD para estar sempre virado para a câmera mais próxima
            if (closestCam != null)
            {
                Vector3 direction = transform.position - closestCam.transform.position;
                transform.forward = direction.normalized;
            }
        }
    }

    /// <summary>
    /// Verifica se o GameObject contém um BrainComponent e HealthComponent válidos
    /// e se os dados de identidade (ID e tipo de entidade) coincidem.
    /// </summary>
    private bool AlignBrain_ID_ENTITY(GameObject target, EntityType target_type, out BrainComponent brain, out HealthComponent health)
    {
        // Tenta obter os dois componentes
        if (target.TryGetComponent(out brain) && target.TryGetComponent(out health))
        {
            // Confere se o ID e o tipo batem com os esperados
            return brain.identity.ID == id_health && brain.identity.TipoEntidade == target_type;
        }

        // Caso não encontre os componentes ou não bata, zera as referências
        brain = null;
        health = null;
        return false;
    }

    /// <summary>
    /// Atualiza a barra de vida do HUD com um novo valor (entre 0 e 1)
    /// </summary>
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
}
