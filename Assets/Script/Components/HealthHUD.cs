using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : MonoBehaviour
{
    [SerializeField] private IconData iconData;
    private Image imageComponent;
    private void Start()
    {
        if (TryGetComponent(out Image image))
        {
            imageComponent = image;
            imageComponent.sprite = iconData.sprites[iconData.sprites.Count - 1];
            imageComponent.preserveAspect = true;
        }
    }
    public void ChangeIcon(int index)
    {
        //print(index);
        //print(iconData.sprites.Count);
        if (index >= 0 && index < iconData.sprites.Count)
        {
            imageComponent.sprite = iconData.sprites[index];
        }
    }
}
