using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : ComponentBehaviour
{
    // Configurações do HUD de vida
    [SerializeField] private GameObject healthBarObject; // Objeto da barra de vida
    [SerializeField] private IconData iconData;         // Dados de ícones (não utilizado no código atual)
    [SerializeField] private HealthHUDType HUDType;     // Tipo de HUD (PLAYER ou ENEMY)
    [SerializeField] private float enemyHealthBarDuration = 3f; // Duração da barra de vida para inimigos
    
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
                        if (p.TryGetComponent(out BrainComponent brain) && 
                            p.TryGetComponent(out HealthComponent health) &&
                            brain.identity.ID == id_health &&
                            brain.identity.TipoEntidade == EntityType.PLAYER)
                        {
                            // Atualiza o slider com os valores de vida atuais
                            if (health.TryGetAttribute("MAX_health", out float max_Health) && 
                                health.TryGetAttribute("health", out float health_v))
                                slider.value = health_v / max_Health;
                        }
                    }
                    break;
                    
                case HealthHUDType.ENEMY:
                    // Preparação para futura implementação
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

    // Método público para atualizar o valor do slider
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
}