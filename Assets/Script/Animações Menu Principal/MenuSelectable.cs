using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectable : 
MonoBehaviour, ISelectHandler, IPointerEnterHandler

{
    
    Button button;

  void Awake()
  {
    button = GetComponent<Button>();
    
  }

  public void OnSelect(BaseEventData eventData)
    {
        if(MenuSelectionCursor.Instance != null)
           MenuSelectionCursor.Instance.MoveTo(button);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!button.interactable)
           return;

        EventSystem.current.SetSelectedGameObject(gameObject);

        if(MenuSelectionCursor.Instance != null)
           MenuSelectionCursor.Instance.MoveTo(button);
    }
}
