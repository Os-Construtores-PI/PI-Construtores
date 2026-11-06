using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PandoraPhaseSelection : MonoBehaviour,

   ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler,
    ISubmitHandler,
    IPointerClickHandler
{
  [Header("Posição Horizontal")]
  [SerializeField] private float _horizontalPosition;

  [Header("Ação após os efeitos")]
  [SerializeField] private UnityEvent _onPhaseConfirmed;

  private Button _button;
  private bool _isConfirming;

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

  public void OnSubmit(BaseEventData eventData)
  {
    ConfirmPhase();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Left)
      return;

    ConfirmPhase();
  }

  private void ConfirmPhase()
  {
    if (_button == null || !_button.interactable)
      return;

    if (_isConfirming)
      return;

    if (PhaseSelectionEffects.Instance == null)
    {
      Debug.LogError("[PandoraPhaseSelection] PhaseSelectionEffects não encontrado!");
      return;
    }

    _isConfirming = true;

    Debug.Log("[PandoraPhaseSelection] Fase confirmada!");

    // Executa os efeitos e aguarda sua conclusão.
    PhaseSelectionEffects.Instance.Show(() =>
    {
      Debug.Log("[PandoraPhaseSelection] Executando ação após os efeitos!");

      _onPhaseConfirmed?.Invoke();
    });
  }

  public void OnDeselect(BaseEventData eventData)
  {
    if (_isConfirming)
      return;

    GameObject nextSelected = eventData.selectedObject;

    if (nextSelected != null &&
        nextSelected.GetComponent<PandoraPhaseSelection>() != null)
      return;

    if (PhaseSelectionEffects.Instance != null)
      PhaseSelectionEffects.Instance.Hide();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (_button == null || !_button.interactable)
      return;

    EventSystem.current.SetSelectedGameObject(gameObject);
  }
}
