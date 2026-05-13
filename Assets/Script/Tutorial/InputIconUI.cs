using UnityEngine;
using UnityEngine.UI;

public class InputIconUI : MonoBehaviour
{
  [SerializeField]
  private DeviceSpriteManager.InputIconType iconType;

  [SerializeField]
  private Image targetImage;

  // Start is called once before the first execution of Update after the MonoBehaviour is created

  private void Awake()
  {
    if (targetImage == null)
      targetImage = GetComponent<Image>();
  }

  private void OnEnable()
  {
    Atualizar(DeviceSpriteManager.Instance.GetCurrentDevice());

    DeviceSpriteManager.Instance.OnDeviceChanged += Atualizar;
  }

  private void OnDisable()
  {
    if (DeviceSpriteManager.Instance != null)
      DeviceSpriteManager.Instance.OnDeviceChanged -= Atualizar;
  }

  private void Atualizar(string device)
  {
    if (targetImage == null)
      return;

    targetImage.sprite = DeviceSpriteManager.Instance.GetSprite(iconType);
  }
}
