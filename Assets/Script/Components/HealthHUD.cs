using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : ComponentBehaviour
{
    // Configurações do HUD de vida
    [SerializeField] private GameObject healthBarObject; // Objeto da barra de vida
    [SerializeField] private IconData iconData;         // Dados de ícones (não utilizado no código atual)
    [SerializeField] public HealthHUDType HUDType;     // Tipo de HUD (PLAYER ou ENEMY)    
    public Transform enemy_target;  // Alvo (transform) do inimigo para seguir
    public int id_health = 0;      // ID do personagem associado a este HUD

    private Slider slider;          // Componente Slider que mostra a vida

    private void Start()
    {
        // Tenta obter o componente Slider no mesmo GameObject
        if (TryGetComponent(out Slider sl))
        {
            slider = sl;

            // Configuração baseada no tipo de HUD
            switch (HUDType)
            {
                case HealthHUDType.PLAYER:
                    // Busca todos os jogadores na cena
                    var players = GameObject.FindGameObjectsWithTag("Player");
                    foreach (var p in players)
                    {
                        // Verifica se é o jogador correto pelo ID
                        if (AlignBrain_ID_ENTITY(p,EntityType.PLAYER, out BrainComponent brain,out HealthComponent health))
                        {
                            // Atualiza o slider com os valores de vida atuais
                            if (health.TryGetAttribute("MAX_health", out float max_Health) &&
                                health.TryGetAttribute("health", out float health_v))
                                slider.value = health_v / max_Health;
                        }
                    }
                    break;

                case HealthHUDType.ENEMY:
                    if (enemy_target)
                    {
                        GameObject enemy = enemy_target.parent.gameObject;
                        if (AlignBrain_ID_ENTITY(enemy, EntityType.ENEMY, out BrainComponent brain, out HealthComponent health))
                        {
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
        // Comportamento específico para HUD de inimigos
        if (HUDType == HealthHUDType.ENEMY && enemy_target)
        {
            // Posiciona o HUD na posição do inimigo
            transform.position = enemy_target.position;

            // Encontra a câmera mais próxima para orientação
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

            // Orienta o HUD para ficar sempre virado para a câmera mais próxima
            if (closestCam != null)
            {
                Vector3 direction = transform.position - closestCam.transform.position;
                transform.forward = direction.normalized;
            }
        }
    }

    private bool AlignBrain_ID_ENTITY(GameObject target,EntityType target_type, out BrainComponent brain, out HealthComponent health)
    {
        // Tenta obter os dois componentes
        if (target.TryGetComponent(out brain) && target.TryGetComponent(out health))
        {
            // Verifica se o ID e o tipo de entidade batem
            return brain.identity.ID == id_health && brain.identity.TipoEntidade == target_type;
        }

        // Se não conseguiu pegar os dois componentes, define os out como null
        brain = null;
        health = null;
        return false;
    }



    // Método público para atualizar o valor do slider
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
}