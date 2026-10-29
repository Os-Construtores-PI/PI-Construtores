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

  private static GameObject _lastSelectedObject;
  private static bool _navigationStarted;

  private void OnEnable()
  {
    _navigationStarted = false;
    _lastSelectedObject = null;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    PlayHoverSound();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    PlayClickSound();
  }

  public void OnSelect(BaseEventData eventData)
  {
    // Ignora a seleção automática inicial do menu.
    if (!_navigationStarted)
    {
      _navigationStarted = true;
      _lastSelectedObject = gameObject;
      return;
    }

    // Só toca quando a seleção muda para outro botão.
    if (_lastSelectedObject != gameObject)
    {
      _lastSelectedObject = gameObject;
      PlayHoverSound();
    }
  }

  public void OnDeselect(BaseEventData eventData)
  {
    // Não limpamos a seleção aqui, pois o próximo botão
    // precisa ser comparado com o botão anteriormente selecionado.
  }

  public void OnSubmit(BaseEventData eventData)
  {
    PlayClickSound();
  }

  private void PlayHoverSound()
  {
    if (_uiAudioConfig == null ||
        AudioManager.Instance == null)
      return;

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
  }

  private void PlayClickSound()
  {
    if (_uiAudioConfig == null ||
        AudioManager.Instance == null)
      return;

    if (_uiAudioConfig.Click == null)
      return;

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  private void OnDisable()
  {
    if (_lastSelectedObject == gameObject)
      _lastSelectedObject = null;
  }
}
