using TMPro;
using UnityEngine;

public class StopwatchHUD : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI _stopwatchOutput;

  private float _elapsed = 0f;
  public float Elapsed => _elapsed;

  // Convenience para salvar em int segundos, se precisar
  public int TotalSeconds => (int)_elapsed;

  private void Update()
  {
    _elapsed += Time.deltaTime;
    UpdateDisplay();
  }

  private void UpdateDisplay()
  {
    int totalSeconds = (int)_elapsed;
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;
    int milliseconds = (int)((_elapsed - totalSeconds) * 100f);

    _stopwatchOutput.text = $"{minutes:D2}:{seconds:D2}:{milliseconds:D2}";
  }

  public void ResetStopwatch()
  {
    _elapsed = 0f;
  }
}
