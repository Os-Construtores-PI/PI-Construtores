using UnityEngine;
using UnityEngine.EventSystems;
public class UIButtonSound : MonoBehaviour, 
  IPointerEnterHandler, 
  IPointerClickHandler,
  ISelectHandler,
  ISubmitHandler
{
  [SerializeField] private somMenu _somMenu;
  public void OnPointerEnter(PointerEventData eventData)
  {
    if(AudioManager.Instance != null)
    AudioManager.Instance.PlaySFX(_somMenu.hover);
  }
  

  public void OnPointerClick(PointerEventData eventData)
  {
    if(AudioManager.Instance != null)
    AudioManager.Instance.PlaySFX(_somMenu.click);
  }

  public void OnSelect(BaseEventData eventData)
  {
    if(AudioManager.Instance != null)
    AudioManager.Instance.PlaySFX(_somMenu.hover);
  }

  public void OnSubmit(BaseEventData eventData)
  {
     if(AudioManager.Instance != null)
    AudioManager.Instance.PlaySFX(_somMenu.click);
  }
}
