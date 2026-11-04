using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PandoraPhaseSelection : MonoBehaviour,

  ISelectHandler,
  IPointerEnterHandler
{
  private Button _button;

  private void Awake()
  {
    _button = GetComponent<Button>();
  }

  public void OnSelect(BaseEventData eventData)
  {
    if (_button == null || !_button.interactable)
      return;

    if (PandoraNavigationIndicator.Instance == null)
      return;

    PandoraNavigationIndicator.Instance.MoveTo(_button);
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (_button == null || !_button.interactable)
      return;

    EventSystem.current.SetSelectedGameObject(gameObject);
  }
}
