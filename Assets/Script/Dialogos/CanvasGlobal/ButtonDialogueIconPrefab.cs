using UnityEngine;
using UnityEngine.UI;

public class ButtonDialogueIconPrefab : MonoBehaviour
{
    [SerializeField] private DeviceSpriteManager.DialogueButtonType _buttonType;
    private Image _image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        AtualizarSprite();

        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(string device)
    {
        AtualizarSprite();
    }



    private void AtualizarSprite()
    {
        if (_image == null || DeviceSpriteManager.Instance == null)
            return;

        _image.sprite = DeviceSpriteManager.Instance.GetDialogueButtonSprite(_buttonType);
    }
}
