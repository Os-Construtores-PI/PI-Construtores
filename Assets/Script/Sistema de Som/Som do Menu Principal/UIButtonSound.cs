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

  [SerializeField] private bool _usePauseSound = false;

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
    PlayHover();
  }

  public void OnSubmit(BaseEventData eventData)
  {
    if (AudioManager.Instance == null ||
        _uiAudioConfig == null)
      return;

    if (_usePauseSound)
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);
    }
    else
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
    }
  }

  private void PlayHover()
  {
    if (AudioManager.Instance != null &&
        _uiAudioConfig != null)
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
    }
  }
}
