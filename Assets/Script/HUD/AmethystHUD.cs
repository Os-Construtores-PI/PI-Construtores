using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmethystHUD : MonoBehaviour
{
  [Header("Referência de UI")]
  [SerializeField]
  private TMP_Text _amethystText;

  [SerializeField]
  private float _scaleDuration = .5f;

  public void Start()
  {
    DOTween.Init(logBehaviour: LogBehaviour.Verbose, recycleAllByDefault: true);

    if (_amethystText == null)
      _amethystText = GetComponentInChildren<TMP_Text>();

    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateText);
  }

  public void OnDestroy()
  {
    if (GlobalEventBus.HasInstance)
      GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateText);
  }

  private void UpdateText(int newCount)
  {
    if (_amethystText != null)
    {
      VisualAmethyst(newCount);
      return;
    }
  }

  private void VisualAmethyst(int newCount)
  {
    Sequence sequence = DOTween.Sequence();
    sequence.AppendCallback(() =>
    {
      _amethystText.transform.DOKill(complete: false);
      _amethystText.transform.localScale = Vector3.one;
      _amethystText
        .transform.DOPunchScale(Vector3.one * 2f, _scaleDuration, 1, 1)
        .SetUpdate(true)
        .SetLink(_amethystText.gameObject);
      _amethystText.text = newCount.ToString("00");
    });
    sequence.Play();
  }

  public void SetupAmethysts()
  {
    UpdateText(0);
  }
}
