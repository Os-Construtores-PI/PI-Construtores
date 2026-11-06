using DG.Tweening;
using System;
using UnityEngine;

public class PhaseSelectionEffects : MonoBehaviour
{

  public static PhaseSelectionEffects Instance { get; private set; }

  [Header("Referências")]
  [SerializeField] private CanvasGroup _effectsGroup;

  [Header("Configuração")]
  [SerializeField] private float _fadeDuration = 0.3f;
  [SerializeField] private float _visibleDuration = 0.8f;
  [SerializeField] private Ease _fadeEase = Ease.OutQuad;

  private Sequence _showSequence;
  private bool _isShowing;

  private void Awake()
  {
    Instance = this;

    if (_effectsGroup == null)
      _effectsGroup = GetComponent<CanvasGroup>();

    if (_effectsGroup == null)
    {
      Debug.LogError("[PhaseSelectionEffects] CanvasGroup não encontrado!");
      return;
    }

    _effectsGroup.alpha = 0f;
    _effectsGroup.interactable = false;
    _effectsGroup.blocksRaycasts = false;
  }

  public void Show(Action onComplete = null)
  {
    if (_effectsGroup == null || _isShowing)
      return;

    _isShowing = true;

    _effectsGroup.gameObject.SetActive(true);

    _showSequence?.Kill();
    _effectsGroup.DOKill();

    _showSequence = DOTween.Sequence()
        .SetUpdate(true)
        .Append(
            _effectsGroup.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
        )
        .AppendInterval(_visibleDuration)
        .OnComplete(() =>
        {
          Debug.Log("[PhaseSelectionEffects] Efeitos concluídos!");

          _isShowing = false;

          onComplete?.Invoke();
        });
  }

  public void Hide()
  {
    if (_effectsGroup == null)
      return;

    _showSequence?.Kill();
    _effectsGroup.DOKill();

    _isShowing = false;

    _effectsGroup.DOFade(0f, _fadeDuration)
        .SetEase(_fadeEase)
        .SetUpdate(true);
  }

  private void OnDestroy()
  {
    _showSequence?.Kill();

    if (_effectsGroup != null)
      _effectsGroup.DOKill();

    if (Instance == this)
      Instance = null;
  }
}
