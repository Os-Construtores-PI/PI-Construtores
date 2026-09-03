using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound
  : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler,
    ISelectHandler,
    ISubmitHandler
{
  [SerializeField]
  private UIAudioConfig _uiAudioConfig;

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  public void OnSelect(BaseEventData eventData)
  {
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
  }

  public void OnSubmit(BaseEventData eventData)
  {
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }
}
