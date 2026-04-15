using UnityEngine;
using UnityEngine.EventSystems;
public class UIButtonSound : MonoBehaviour, 
  IPointerEnterHandler, 
  IPointerClickHandler,
  ISelectHandler,
  ISubmitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
  {
    MenuAudioManager.Instance.PlayHover();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    MenuAudioManager.Instance.PlayClick();
  }

  public void OnSelect(BaseEventData eventData)
  {
    MenuAudioManager.Instance.PlayHover();
  }

  public void OnSubmit(BaseEventData eventData)
  {
    MenuAudioManager.Instance.PlayClick();
  }
}
