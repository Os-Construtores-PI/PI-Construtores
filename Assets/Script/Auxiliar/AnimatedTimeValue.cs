using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AnimatedTimeValue : MonoBehaviour
{
  [Header("Referências")]
  [SerializeField]
  private TextMeshProUGUI _textOutput;

  [Header("Configurações da Animação")]
  [Tooltip("Tempo em segundos que a animação leva para atingir o novo valor.")]
  [SerializeField]
  private float _duration = 1.0f;

  [Tooltip("Suavização. 'OutCubic' ou 'Linear' são os mais comuns para cronômetros.")]
  [SerializeField]
  private Ease _ease = Ease.OutCubic;

  [Header("Formatação")]
  [Tooltip("Se verdadeiro, exibe MM:SS:cs (centésimos). Se falso, apenas MM:SS.")]
  [SerializeField]
  private bool _showCentiseconds = true;

  private Tween _currentTween;

  public void SetValue(float newTimeInSeconds)
  {
    _currentTween?.Kill();

    float startValue = ParseTimeFromString(_textOutput.text);

    _currentTween = DOVirtual
      .Float(startValue, newTimeInSeconds, _duration, value => _textOutput.text = FormatTime(value))
      .SetEase(_ease)
      .SetUpdate(true);
  }

  public void SetValueImmediate(float newTimeInSeconds)
  {
    _currentTween?.Kill();
    _textOutput.text = FormatTime(newTimeInSeconds);
  }

  private float ParseTimeFromString(string timeString)
  {
    if (string.IsNullOrEmpty(timeString))
      return 0f;

    string[] parts = timeString.Split(':');

    if (parts.Length >= 2)
    {
      if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
      {
        float centiseconds = 0f;
        if (parts.Length >= 3)
        {
          int.TryParse(parts[2], out int cs);
          centiseconds = cs / 100f;
        }

        return (minutes * 60f) + seconds + centiseconds;
      }
    }

    return 0f;
  }

  private string FormatTime(float timeInSeconds)
  {
    // Garante que não haja valores negativos visuais
    timeInSeconds = Mathf.Max(0f, timeInSeconds);

    int totalSeconds = (int)timeInSeconds;
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;

    if (_showCentiseconds)
    {
      int centiseconds = (int)((timeInSeconds - totalSeconds) * 100f) % 100;
      return $"{minutes:D2}:{seconds:D2}:{centiseconds:D2}";
    }
    else
    {
      return $"{minutes:D2}:{seconds:D2}";
    }
  }

  private void OnDestroy()
  {
    _currentTween?.Kill();
  }
}
