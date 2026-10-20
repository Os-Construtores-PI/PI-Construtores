using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
  private AnimatedIntValue _scoreOutput;

  [SerializeField]
  private AnimatedIntValue _previewScoreOutput;

  [SerializeField]
  private AnimatedTimeValue _timeOutput;

  [SerializeField]
  private TextMeshProUGUI _uuidOutput;

  private bool _canInput = false;

  // ═══════════════════════════════════════════════════════════════════════════
  // Unity Events
  // ═══════════════════════════════════════════════════════════════════════════

  private void Awake()
  {
    BuildRankSpriteMap();
  }

  private void Update()
  {
    ListeningForInput();
  }

  private void OnEnable()
  {
    DeviceInputManager.Instance.ForceRefresh();
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

  public void Populate(int score, int previewScore, float time, string uuid, RankType rank)
  {
    HideRank();
    _canInput = false;

    int pending = 3;

    void Done()
    {
      if (--pending == 0)
        _canInput = true;
    }

    SetScore(
      score,
      () =>
      {
        SetRank(rank);
        Done();
      }
    );
    SetPreviewScore(previewScore, Done);
    SetTime(time, Done);
    SetUUID(uuid);
  }

  private void SetPreviewScore(int previewScore, Action onComplete = null)
  {
    if (!_previewScoreOutput)
    {
      onComplete?.Invoke();
      return;
    }

    _previewScoreOutput.SetValue(previewScore).OnComplete(() => onComplete?.Invoke());
  }

  private void SetTime(float seconds, Action onComplete = null)
  {
    if (!_timeOutput)
    {
      onComplete?.Invoke();
      return;
    }

    _timeOutput.SetValue(seconds).OnComplete(() => onComplete?.Invoke());
  }

  private void SetScore(int score, Action onComplete = null)
  {
    if (!_scoreOutput)
    {
      onComplete?.Invoke();
      return;
    }

    _scoreOutput.SetValue(score).OnComplete(() => onComplete?.Invoke());
  }

  private void HideRank()
  {
    if (_rankOutput)
      _rankOutput.gameObject.SetActive(false);
  }

  private void SetRank(RankType rank)
  {
    if (!_rankOutput)
      return;

    if (_rankSpriteMap == null)
      BuildRankSpriteMap();

    if (_rankSpriteMap.TryGetValue(rank, out var sprite))
      _rankOutput.sprite = sprite;
    else
      Debug.LogWarning($"[EndGamePanel] Nenhum sprite mapeado para o rank {rank}.");

    _rankOutput.gameObject.SetActive(true);
  }

  private void SetUUID(string uuid)
  {
    if (!_uuidOutput)
      return;

    _uuidOutput.text = uuid;
  }

  private void ListeningForInput()
  {
    if (!_canInput)
      return;
    bool advancePressed = false;
    bool resetPressed = false;
    if (Keyboard.current != null)
    {
      if (Keyboard.current.fKey.wasPressedThisFrame)
        advancePressed = true;
      if (Keyboard.current.rKey.wasPressedThisFrame)
        resetPressed = true;
    }

    if (Gamepad.current != null)
    {
      if (Gamepad.current.aButton.wasPressedThisFrame)
        advancePressed = true;
      if (Gamepad.current.bButton.wasPressedThisFrame)
        resetPressed = true;
    }

    if (advancePressed)
    {
      OnAdvance();
    }
    else if (resetPressed)
    {
      OnReset();
    }
  }

  private void OnAdvance()
  {
    Debug.Log("[EndGamePanel] Avançar acionado!");
    Time.timeScale = 1;
    SceneManager.LoadScene(Constants.SceneNames.MainMenu);
  }

  private void OnReset()
  {
    Debug.Log("[EndGamePanel] Resetar acionado!");
    Time.timeScale = 1;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    GameContext.ShowStageIntro = true;
  }
}
