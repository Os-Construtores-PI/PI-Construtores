using TMPro;
using UnityEngine;

public class RankingEntry : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI _rankingOutput;

  [SerializeField]
  private TextMeshProUGUI _idOutput;

  [SerializeField]
  private TextMeshProUGUI _scoreOutput;

  [SerializeField]
  private TextMeshProUGUI _timeOutput;

  public void SetData(int rankingPosition, string finishUUID, int score, float time)
  {
    _rankingOutput.text = rankingPosition.ToString();
    _idOutput.text = finishUUID;
    _scoreOutput.text = score.ToString();
    _timeOutput.text = FormatTime(time);
  }

  private static string FormatTime(float seconds)
  {
    var span = System.TimeSpan.FromSeconds(seconds);
    return $"{(int)span.TotalMinutes:00}:{span.Seconds:00}.{span.Milliseconds / 10:00}";
  }
}
