using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : MonoBehaviour
{
    [SerializeField] private IconData iconData;
    [SerializeField] private int health_id_player;
    private Slider slider;
    private void Start()
    {
        if (TryGetComponent(out Slider sl))
        {
            slider = sl;
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                if (p.TryGetComponent(out BrainComponent brain) && p.TryGetComponent(out HealthComponent health) &&
                    brain.identity.ID == health_id_player &&
                    brain.identity.TipoEntidade == EntityType.PLAYER)
                {
                    slider.value = health.GetAttribute<float>("health") / health.GetAttribute<float>("MAX_health");
                }
            }
        }
    }
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
}
