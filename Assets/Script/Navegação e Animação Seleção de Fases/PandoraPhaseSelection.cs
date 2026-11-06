using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PandoraPhaseSelection : MonoBehaviour,

   ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler
{
    [Header("Posição Horizontal")]
    [SerializeField] private float _horizontalPosition;

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

        PandoraNavigationIndicator.Instance.MoveTo(_horizontalPosition);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Não esconde quando a próxima seleção também
        // pertence a outro botão de fase.
        GameObject nextSelected = eventData.selectedObject;

        if (nextSelected != null &&
            nextSelected.GetComponent<PandoraPhaseSelection>() != null)
            return;

        if (PandoraNavigationIndicator.Instance != null)
            PandoraNavigationIndicator.Instance.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button == null || !_button.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
