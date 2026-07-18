using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DeviceSpriteHint : MonoBehaviour
{


    [SerializeField] private Image targetImage;

    [Header("Sprites")]
    [SerializeField] private Sprite keyboardSprite;
    [SerializeField] private Sprite xboxSprite;
    [SerializeField] private Sprite playstationSprite;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (DeviceSpriteManager.Instance == null)
            return;

        DeviceSpriteManager.Instance.OnDeviceChanged += UpdateSprite;
        DeviceSpriteManager.Instance.RefreshCurrentDevice();
        UpdateSprite(DeviceSpriteManager.Instance.GetCurrentDevice());
    }

    private void OnDisable()
    {
        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged -= UpdateSprite;
    }

    private void UpdateSprite(string device)
    {
        switch (device)
        {
            case "Keyboard":
                targetImage.sprite = keyboardSprite;
                break;

            case "Xbox":
                targetImage.sprite = xboxSprite;
                break;

            case "Playstation":
                targetImage.sprite = playstationSprite;
                break;
        }

        // Caso os sprites tenham tamanhos diferentes
        targetImage.SetNativeSize();
    }
}
