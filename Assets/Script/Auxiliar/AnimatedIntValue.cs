using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AnimatedIntValue : MonoBehaviour
{
  [Header("Referências")]
  [SerializeField]
  private TextMeshProUGUI _textOutput;

  [Header("Configurações da Animação")]
  [Tooltip("Tempo em segundos que a animação leva para ir do valor atual até o novo valor.")]
  [SerializeField]
  private float _duration = 0.6f;

  [Tooltip(
    "A suavização da animação. 'OutCubic' ou 'OutExpo' dão a sensação clássica de contador de score."
  )]
  [SerializeField]
  private Ease _ease = Ease.OutCubic;

  [Tooltip("Quantidade de zeros à esquerda (ex: 6 para 000000, 8 para 00000000).")]
  [SerializeField, Range(1, 10)]
  private int _digitCount = 8;

  private Tween _currentTween;

  public void SetValue(int newValue)
  {
    _currentTween?.Kill();

    int startValue = 0;
    if (!string.IsNullOrEmpty(_textOutput.text))
    {
      int.TryParse(_textOutput.text, out startValue);
    }

    string format = "D" + _digitCount;

    _currentTween = DOVirtual
      .Int(startValue, newValue, _duration, value => _textOutput.text = value.ToString(format))
      .SetEase(_ease)
      .SetUpdate(true);
  }

  public void SetValueImmediate(int newValue)
  {
    _currentTween?.Kill();
    string format = "D" + _digitCount;
    _textOutput.text = newValue.ToString(format);
  }

  private void OnDestroy()
  {
    _currentTween?.Kill();
  }
}
