using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectable : 
MonoBehaviour, ISelectHandler, IPointerEnterHandler


{
    
    Button button;

    [SerializeField] private PreviewSettings preview;
    [SerializeField] GameObject _spriteIndicador;

  public static bool CanSeletc;

  void Awake()
  {
    button = GetComponent<Button>();

    if(_spriteIndicador != null)
       _spriteIndicador.SetActive(false);
    
  }

  public void MostrarSprite()
  {
    if(_spriteIndicador != null)
       _spriteIndicador.SetActive(true);
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

    public void ForcePreview()
   {
      if(preview != null)
         MenuPreview.Instance.Show(preview);
   }
}

