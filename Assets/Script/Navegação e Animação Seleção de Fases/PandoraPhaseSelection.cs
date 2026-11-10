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

  private Vector3 _startsOriginalScale;
  private Tween _startsTween;

  [Header("Gradiente")]
  [SerializeField] private GameObject _gradiente;

  private RectTransform _gradienteRect;
  private Vector2 _gradienteOriginalPosition;
  private Tween _gradienteTween;

  [SerializeField] private float _gradienteMoveDistance = 150f;
  [SerializeField] private float _gradienteMoveDuration = 0.4f;


  private void Awake()
  {
    _button = GetComponent<Button>();

    if (_starts != null)
    {
      _startsOriginalScale = _starts.transform.localScale;
      _starts.SetActive(false);
    }

    if (_gradiente != null)
    {
      _gradienteRect = _gradiente.GetComponent<RectTransform>();

      if (_gradienteRect != null)
      {
        _gradienteOriginalPosition = _gradienteRect.anchoredPosition;
      }

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
    if (_starts == null) return;

    _startsTween?.Kill();

    Transform starsTransform = _starts.transform;

    _starts.SetActive(true);
    starsTransform.localScale = Vector3.zero;

    _startsTween = DOTween.Sequence()
        .AppendInterval(0.1f)
        .Append(
            starsTransform.DOScale(
                _startsOriginalScale * 1.25f,
                0.2f
            ).SetEase(Ease.OutBack)
        )
        .Append(
            starsTransform.DOScale(
                _startsOriginalScale,
                0.15f
            ).SetEase(Ease.OutQuad)
        );
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
    if (_gradiente == null || _gradienteRect == null)
      return;

    _gradienteTween?.Kill();
    _startsTween?.Kill();

    _gradiente.SetActive(true);

    _gradienteRect.anchoredPosition =
        _gradienteOriginalPosition + Vector2.up * _gradienteMoveDistance;

    // Inicia a descida do gradiente.
    Sequence sequence = DOTween.Sequence()
        .SetUpdate(true);

    sequence.Append(
        _gradienteRect
            .DOAnchorPos(_gradienteOriginalPosition, _gradienteMoveDuration)
            .SetEase(Ease.OutCubic)
    );

    _gradienteTween = sequence;

    // Faz as estrelas aparecerem quase junto com o gradiente.
    DOVirtual.DelayedCall(0.1f, ShowStarts)
        .SetUpdate(true);

  }

  public float GetGradienteDuration()
  {
    return Mathf.Max(_gradienteMoveDuration, 0.55f);
  }

  public float GetBlinkStartDelay()
  {
    return Mathf.Max(0f, GetGradienteDuration() - 0.2f);
  }

  public float GetBlinkDuration()
  {
    return 0.2f * 5f;
  }
}
