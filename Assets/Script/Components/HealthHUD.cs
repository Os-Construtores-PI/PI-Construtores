using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : ComponentBehaviour
{
    [SerializeField] private IconData iconData;
    [SerializeField] private HealthHUDType HUDType;
    public Transform enemy_target;
    public int id_health = 0;
    private Slider slider;
    private void Start()
    {
        if (TryGetComponent(out Slider sl))
        {
            slider = sl;
            switch (HUDType)
            {
                case HealthHUDType.PLAYER:
                    var players = GameObject.FindGameObjectsWithTag("Player");
                    foreach (var p in players)
                    {
                        if (p.TryGetComponent(out BrainComponent brain) && p.TryGetComponent(out HealthComponent health) &&
                            brain.identity.ID == id_health &&
                            brain.identity.TipoEntidade == EntityType.PLAYER)
                        {
                            if (health.TryGetAttribute("MAX_health", out float max_Health) && health.TryGetAttribute("health", out float health_v))
                                slider.value = health_v / max_Health;
                        }
                    }
                    break;
                case HealthHUDType.ENEMY:
                // Preparação futura se caso precisarf
                    break;

                
            }
        }
    }
    public void Update()
    {
        if (HUDType == HealthHUDType.ENEMY && enemy_target)
        {
            transform.Translate(enemy_target.position + new Vector3(0, 5, 0));
        }   
    }
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
}
