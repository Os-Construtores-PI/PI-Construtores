using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PandoraPhaseSelection : MonoBehaviour,

   ISelectHandler,
    IPointerEnterHandler,
    ISubmitHandler,
    IPointerClickHandler
{
  [Header("Posição Horizontal")]
  [SerializeField] private float _horizontalPosition;

  [Header("Ação ao Confirmar")]
  [SerializeField] private UnityEvent _onPhaseConfirmed;

  private Button _button;
  private bool _isConfirming;

  [Header("Efeitos Visuais")]
  [SerializeField] private GameObject _starts;


  [Header("Gradiente")]
  [SerializeField] private GameObject _gradiente;

  private void Awake()
  {
    _button = GetComponent<Button>();

    if (_starts != null )
    {
      _starts.SetActive(false);
    }

    if (_gradiente != null)
    {
      _gradiente.SetActive(false);
    }
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

    _isConfirming = true;

    Debug.Log("[PandoraPhaseSelection] Fase confirmada!");

    _onPhaseConfirmed?.Invoke();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (_button == null || !_button.interactable)
      return;

    if (EventSystem.current != null)
      EventSystem.current.SetSelectedGameObject(gameObject);
  }

  public void ShowStarts()
  {
    if (_starts != null)
    {
      _starts.SetActive(true);
    }
  }

  public void HideStarts()
  {
    if (_starts != null)
    {
      _starts.SetActive(false);
    }
  }

  public void ShowGradiente()
  {
    if (_gradiente != null)
    {
      _gradiente.SetActive(true);
    }
  }

}
