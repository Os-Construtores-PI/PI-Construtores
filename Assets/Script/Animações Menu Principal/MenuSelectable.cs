using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectable : 
MonoBehaviour, ISelectHandler, IPointerEnterHandler


{
    
    Button button;

    [SerializeField] private PreviewSettings preview;

  public static bool CanSeletc;

  void Awake()
  {
    button = GetComponent<Button>();
    
  }

  public void OnSelect(BaseEventData eventData)
    {
    if (!CanSeletc)
      return;

        if(MenuSelectionCursor.Instance != null)
           MenuSelectionCursor.Instance.MoveTo(button);
          
           MenuPreview.Instance.Show(preview);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {

       if(!CanSeletc)
        return;

        if(!button.interactable)
           return;

        EventSystem.current.SetSelectedGameObject(gameObject);

        if(MenuSelectionCursor.Instance != null)
           MenuSelectionCursor.Instance.MoveTo(button);

          // MenuPreview.Instance.Show(preview);
    }
}

