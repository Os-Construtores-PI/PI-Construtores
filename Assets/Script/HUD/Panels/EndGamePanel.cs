using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePanel : MonoBehaviour
{
  // ─── Rank ────────────────────────────────────────────────────────────────

  [Header("Rank")]
  [SerializeField]
  private Image _rankOutput;

  [SerializeField]
  private List<RankSpriteEntry> _rankSprites = new();

  private Dictionary<RankType, Sprite> _rankSpriteMap;

  // ─── Stats ───────────────────────────────────────────────────────────────

  [Header("Stats")]
  [SerializeField]
  private TextMeshProUGUI _scoreOutput;

  [SerializeField]
  private TextMeshProUGUI _previewScoreOutput;

  [SerializeField]
  private TextMeshProUGUI _timeOutput;

  // ─── Botão ───────────────────────────────────────────────────────────────

  [Header("Botão")]
  [SerializeField]
  private Button _gotoMenuButton;

  // ═══════════════════════════════════════════════════════════════════════════
  // Unity Events
  // ═══════════════════════════════════════════════════════════════════════════

  private void Awake()
  {
    BuildRankSpriteMap();
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Rank → Sprite
  // ═══════════════════════════════════════════════════════════════════════════

  private void BuildRankSpriteMap()
  {
    _rankSpriteMap = new Dictionary<RankType, Sprite>();

    foreach (var entry in _rankSprites)
    {
      if (entry.Sprite == null)
      {
        Debug.LogWarning($"[EndGamePanel] Sprite não atribuído para o rank {entry.Rank}.");
        continue;
      }

      if (!_rankSpriteMap.TryAdd(entry.Rank, entry.Sprite))
        Debug.LogWarning(
          $"[EndGamePanel] Rank {entry.Rank} duplicado na lista — mantendo o primeiro valor."
        );
    }
  }

  public static RankType CalculateRank(int score, int maxScore)
  {
    if (maxScore <= 0)
      return RankType.D;

    float ratio = (float)score / maxScore;

    return ratio switch
    {
      >= 0.95f => RankType.S,
      >= 0.8f => RankType.A,
      >= 0.6f => RankType.B,
      >= 0.4f => RankType.C,
      _ => RankType.D,
    };
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Preenchimento dos Outputs
  // ═══════════════════════════════════════════════════════════════════════════

  public void Populate(int score, int previewScore, float time, RankType rank)
  {
    SetScore(score);
    SetPreviewScore(previewScore);
    SetTime(time);
    SetRank(rank);
  }

  public void SetScore(int score)
  {
    if (_scoreOutput)
      _scoreOutput.text = score.ToString("D8");
  }

  public void SetPreviewScore(int previewScore)
  {
    if (_previewScoreOutput)
      _previewScoreOutput.text = previewScore.ToString("D8");
  }

  public void SetTime(float seconds)
  {
    if (!_timeOutput)
      return;

    TimeSpan span = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
    _timeOutput.text = span.Hours > 0 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"mm\:ss\.ff");
  }

  public void SetRank(RankType rank)
  {
    if (!_rankOutput)
      return;

    if (_rankSpriteMap == null)
      BuildRankSpriteMap();

    if (_rankSpriteMap.TryGetValue(rank, out var sprite))
      _rankOutput.sprite = sprite;
    else
      Debug.LogWarning($"[EndGamePanel] Nenhum sprite mapeado para o rank {rank}.");
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Botão
  // ═══════════════════════════════════════════════════════════════════════════

  public void BindGotoMenuButton(Action onClick)
  {
    if (!_gotoMenuButton)
      return;

    _gotoMenuButton.onClick.RemoveAllListeners();
    if (onClick != null)
      _gotoMenuButton.onClick.AddListener(() => onClick());
  }
}
