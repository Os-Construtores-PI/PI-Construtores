using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants.PlayerShakes;
using static QualityOfLife;

public class HudDirector : MonoBehaviour
{
  // ─── Constantes ────────────────────────────────────────────────────────────

  private const float TELEPORT_FADE_SECONDS = 1f;

  private const float PANEL_TWEEN_DURATION = 0.25f;
  private const float PANEL_FADE_OPACITY = 0.8f;

  // ─── Campos Serializados ────────────────────────────────────────────────────

  [Header("Icons")]
  [SerializeField]
  private List<IconImage> _icons = new();

  [Header("Audio")]
  [SerializeField]
  private UIAudioConfig _uiAudioConfig;

  [SerializeField]
  private BackgroundMusicConfig _backgroundMusicConfig;

  [Header("Imagens de popup de combo")]
  [SerializeField]
  private List<ComboPopupImage> _popupComboImages = new();

  [Header("Imagens de popup de combo máximo")]
  [SerializeField]
  private Sprite _slamSprite;

  [SerializeField]
  private Sprite _whooshSprite;

  [Header("Punch Panel Settings")]
  [SerializeField]
  private PunchPanelSettings _comboPunchSettings = PunchPanelSettings.Default;

  [SerializeField]
  private PunchPanelSettings _maxComboPunchSettings = PunchPanelSettings.Default;

  // ─── Estado Interno ─────────────────────────────────────────────────────────

  private readonly Dictionary<int, Dictionary<HudPanelType, List<GameObject>>> canvasMap = new();

  private readonly Dictionary<int, TextMeshProUGUI> interactionTexts = new();
  private readonly Dictionary<int, Image> interactionImages = new();
  private readonly Dictionary<int, Sprite> originalSprites = new();
  private readonly Dictionary<int, CameraLogic> _playerCachedCameras = new();
  private readonly Dictionary<int, StopwatchHUD> _playerCachedStopwatches = new();
  private Dictionary<int, int> _playerCachedScores = new();

  private readonly Dictionary<int, CinemachineBasicMultiChannelPerlin> _playerNoises = new();

  private readonly Dictionary<int, CancellationTokenSource> _shakeCts = new();

  private readonly Dictionary<(int, HudPanelType), Sequence> _tempPanelSequences = new();

  // ═══════════════════════════════════════════════════════════════════════════
  // Acesso Público
  // ═══════════════════════════════════════════════════════════════════════════

  public CameraLogic GetCameraScript(int id) => _playerCachedCameras[id];

  // ═══════════════════════════════════════════════════════════════════════════
  // Unity Events
  // ═══════════════════════════════════════════════════════════════════════════

  private void OnEnable()
  {
    if (!GlobalEventBus.HasInstance)
      return;

    GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
    GlobalEventBus.Instance.Cinematic.AddListener(TriggerCinematicBars);
    GlobalEventBus.Instance.Teleport.AddListener(TeleportFade);
    GlobalEventBus.Instance.Death.AddListener(DeathPanel);
    GlobalEventBus.Instance.Respawn.AddListener(RespawnPanel);
    GlobalEventBus.Instance.EndGameProcessed.AddListener(EndPanel);
    GlobalEventBus.Instance.LockOnVisibility.AddListener(SetLockOnVisibility);
    GlobalEventBus.Instance.Pause.AddListener(PausePanel);
    GlobalEventBus.Instance.Options.AddListener(OptionsPausePanel);
    GlobalEventBus.Instance.ComboUpdate.AddListener(ComboPanel);
    GlobalEventBus.Instance.MaxComboReached.AddListener(MaxComboPanel);
    GlobalEventBus.Instance.ScoreUpdate.AddListener(ScorePanel);
  }

  private void OnDisable()
  {
    if (!GlobalEventBus.HasInstance)
      return;

    GlobalEventBus.Instance.ObjectWasSeen.RemoveListener(InteractionPopup);
    GlobalEventBus.Instance.Cinematic.RemoveListener(TriggerCinematicBars);
    GlobalEventBus.Instance.Teleport.RemoveListener(TeleportFade);
    GlobalEventBus.Instance.Death.RemoveListener(DeathPanel);
    GlobalEventBus.Instance.Respawn.RemoveListener(RespawnPanel);
    GlobalEventBus.Instance.EndGameProcessed.RemoveListener(EndPanel);
    GlobalEventBus.Instance.LockOnVisibility.RemoveListener(SetLockOnVisibility);
    GlobalEventBus.Instance.Pause.RemoveListener(PausePanel);
    GlobalEventBus.Instance.Options.RemoveListener(OptionsPausePanel);
    GlobalEventBus.Instance.ComboUpdate.RemoveListener(ComboPanel);
    GlobalEventBus.Instance.MaxComboReached.RemoveListener(MaxComboPanel);
    GlobalEventBus.Instance.ScoreUpdate.RemoveListener(ScorePanel);
  }

  private void OnDestroy()
  {
    foreach (var cts in _shakeCts.Values)
      cts?.Cancel();

    foreach (var cts in _shakeCts.Values)
      cts?.Dispose();

    _shakeCts.Clear();
  }

  private void Start()
  {
    DOTween.Init();
  }

  private void Update()
  {
    if (EventSystem.current.currentSelectedGameObject != null)
      return;

    bool gamepadNavigating =
      Gamepad.current != null
      && (
        Gamepad.current.dpad.ReadValue() != Vector2.zero
        || Gamepad.current.leftStick.ReadValue() != Vector2.zero
      );

    if (gamepadNavigating || Keyboard.current.anyKey.wasPressedThisFrame)
      SelectFirstButton();
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Inicialização
  // ═══════════════════════════════════════════════════════════════════════════

  public GameObject InitializeHUD(Player player, Transform hudParent, GameObject hudPrefab)
  {
    if (player == null || hudPrefab == null || hudParent == null)
      return null;

    int playerID = player.ID;

    GameObject hudInstance = Instantiate(hudPrefab, hudParent);
    hudInstance.name = $"HUD_Player_ID_{playerID}";

    Canvas.ForceUpdateCanvases();

    var panelMap = new Dictionary<HudPanelType, List<GameObject>>(
      HudPanelEqualityComparer.Instance
    );
    CollectPanelsRecursive(hudInstance.transform, panelMap);
    canvasMap[playerID] = panelMap;

    HealthHUD healthHUD = hudInstance.GetComponentInChildren<HealthHUD>();
    if (healthHUD != null)
    {
      healthHUD.BindToPlayer(player);
    }

    BoostHUD boostHUD = hudInstance.GetComponentInChildren<BoostHUD>();
    if (boostHUD != null)
    {
      boostHUD.BindToPlayer(player);
    }

    StopwatchHUD stopwatchHUD = hudInstance.GetComponentInChildren<StopwatchHUD>();
    if (stopwatchHUD != null)
    {
      _playerCachedStopwatches[playerID] = stopwatchHUD;
    }

    if (panelMap.TryGetValue(HudPanelType.InteractionPopup, out var panels) && panels.Count > 0)
    {
      var text = panels[0].GetComponentInChildren<TextMeshProUGUI>();
      var image = panels[0].GetComponent<Image>();

      if (text)
        interactionTexts[playerID] = text;
      if (image)
      {
        interactionImages[playerID] = image;
        originalSprites[playerID] = image.sprite;
      }
    }

    HideAllPanels(playerID);
    return hudInstance;
  }

  /// <summary>
  /// Registra a câmera e noise do cinemachine associada a um jogador.
  /// </summary>
  public void InitializeCamera(int playerID, CameraLogic camera)
  {
    _playerCachedCameras[playerID] = camera;
  }

  public void InitializeNoise(int playerID, CinemachineBasicMultiChannelPerlin noise)
  {
    _playerNoises[playerID] = noise;
  }

  private void HideAllPanels(int playerID)
  {
    HidePanel(HudPanelType.GameOver, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.EndGame, playerID, independent: true, fade: false, instant: true);
    HidePanel(
      HudPanelType.InteractionPopup,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      HudPanelType.TeleportFadePanel,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(HudPanelType.Pause, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.Dialogue, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.Combo, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.MaxComboPopup, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.ComboPopup, playerID, independent: true, fade: false, instant: true);
  }

  /// <summary>
  /// Percorre a hierarquia recursivamente coletando GameObjects com nomes válidos de painel.
  /// </summary>
  private void CollectPanelsRecursive(
    Transform parent,
    Dictionary<HudPanelType, List<GameObject>> map
  )
  {
    foreach (Transform child in parent)
    {
      if (Enum.TryParse<HudPanelType>(child.name, out var panel))
      {
        if (!map.ContainsKey(panel))
          map[panel] = new List<GameObject>();

        map[panel].Add(child.gameObject);
      }

      CollectPanelsRecursive(child, map);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Controle de Painéis
  // ═══════════════════════════════════════════════════════════════════════════

  // ═══════════════════════════════════════════════════════════════════════════
  // Controle de Painéis
  // ═══════════════════════════════════════════════════════════════════════════

  private void HidePanel(
    HudPanelType panel,
    int playerID,
    bool independent,
    bool fade = false,
    bool instant = false
  )
  {
    var roots = GetPanel(playerID, panel);

    foreach (var rootGo in roots)
    {
      if (rootGo == null)
        continue;

      DisableButton(rootGo);

      if (instant)
      {
        // Cancela animações pendentes e define o estado final imediatamente
        rootGo.transform.DOKill();
        foreach (var child in rootGo.GetComponentsInChildren<Transform>(true))
        {
          child.transform.DOKill();
          if (fade && child.gameObject.TryGetComponent(out Image image))
          {
            var c = image.color;
            image.color = new Color(c.r, c.g, c.b, 0f);
          }
        }
        rootGo.transform.localScale = Vector3.zero;
        rootGo.SetActive(false); // Desativa imediatamente, disparando OnDisable
      }
      else
      {
        // Garante que o painel esteja ativo para executar a animação de saída
        rootGo.SetActive(true);

        foreach (var child in rootGo.GetComponentsInChildren<Transform>(true))
        {
          ApplyFadeOut(child.gameObject, fade, instant, independent);
        }

        rootGo.transform.DOKill();
        rootGo
          .transform.DOScale(Vector3.zero, PANEL_TWEEN_DURATION)
          .SetUpdate(UpdateType.Normal, independent)
          .OnComplete(() =>
          {
            // Desativa o GameObject ao final da animação, disparando OnDisable nos scripts
            if (rootGo != null)
            {
              rootGo.SetActive(false);
            }
          });
      }
    }
  }

  public List<GameObject> ShowPanel(
    HudPanelType panel,
    int playerID,
    bool independent,
    bool fade = false
  )
  {
    var roots = GetPanel(playerID, panel);

    foreach (var rootGo in roots)
    {
      if (rootGo == null)
        continue;

      bool wasActive = rootGo.activeSelf;

      rootGo.SetActive(true);
      EnableButton(rootGo);

      foreach (var child in rootGo.GetComponentsInChildren<Transform>(true))
      {
        ApplyFadeIn(child.gameObject, fade, independent);
      }

      rootGo.transform.DOKill();

      if (!wasActive)
      {
        rootGo.transform.localScale = Vector3.zero;
      }

      rootGo
        .transform.DOScale(Vector3.one, PANEL_TWEEN_DURATION)
        .SetUpdate(UpdateType.Normal, independent);
    }

    EventSystem.current.SetSelectedGameObject(null);
    return roots;
  }

  private void ShowPanelTemporary(HudPanelType panel, int playerID, float duration)
  {
    KillTempSequence(panel, playerID);
    ShowPanel(panel, playerID, independent: true);
    ScheduleTempHide(panel, playerID, duration);
  }

  public void PunchPanelTemporary(HudPanelType panel, int playerID, PunchPanelSettings settings)
  {
    KillTempSequence(panel, playerID);
    PunchPanel(panel, playerID, independent: true, settings);
    ScheduleTempHide(panel, playerID, settings.Duration);
  }

  private void KillTempSequence(HudPanelType panel, int playerID)
  {
    var key = (playerID, panel);
    if (_tempPanelSequences.TryGetValue(key, out var existing))
      existing?.Kill();
  }

  private void ScheduleTempHide(HudPanelType panel, int playerID, float duration)
  {
    var key = (playerID, panel);
    _tempPanelSequences[key] = DOTween
      .Sequence()
      .AppendInterval(duration)
      .AppendCallback(() =>
      {
        HidePanel(panel, playerID, independent: true);
        _tempPanelSequences.Remove(key);
      })
      .SetUpdate(UpdateType.Normal, isIndependentUpdate: true);
  }

  public void PunchPanel(
    HudPanelType panel,
    int playerID,
    bool independent,
    PunchPanelSettings settings
  )
  {
    var roots = GetPanel(playerID, panel);

    foreach (var rootGo in roots)
    {
      EnableButton(rootGo);
      rootGo.transform.DOKill();

      rootGo.transform.localScale = Vector3.one;
      rootGo.transform.localRotation = Quaternion.Euler(
        0f,
        0f,
        UnityEngine.Random.Range(-settings.MaxRotationZ, settings.MaxRotationZ)
      );

      rootGo
        .transform.DOPunchScale(
          Vector3.one * settings.Strength,
          settings.TweenDuration,
          vibrato: settings.Vibrato,
          elasticity: settings.Elasticity
        )
        .SetUpdate(UpdateType.Normal, independent);
    }

    EventSystem.current.SetSelectedGameObject(null);
  }

  // ─── Helpers de painel ──────────────────────────────────────────────────────

  private static void DisableButton(GameObject go)
  {
    if (!go.TryGetComponent(out Button button))
      return;
    button.interactable = false;
    EventSystem.current.SetSelectedGameObject(null);
  }

  private static void EnableButton(GameObject go)
  {
    if (go.TryGetComponent(out Button button))
      button.interactable = true;
  }

  private static void ApplyFadeOut(GameObject go, bool fade, bool instant, bool independent)
  {
    if (!fade || !go.TryGetComponent(out Image image))
      return;

    image.raycastTarget = false;

    if (instant)
      image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    else
      image.DOFade(0f, PANEL_TWEEN_DURATION).SetUpdate(UpdateType.Normal, independent);
  }

  private static void ApplyFadeIn(GameObject go, bool fade, bool independent)
  {
    if (!fade || !go.TryGetComponent(out Image image))
      return;

    image.raycastTarget = true;
    image
      .DOFade(PANEL_FADE_OPACITY, PANEL_TWEEN_DURATION)
      .SetUpdate(UpdateType.Normal, independent);
  }

  private static void ApplyScaleOut(GameObject go, bool instant, bool independent)
  {
    if (instant)
    {
      go.transform.DOKill();
      go.transform.localScale = Vector3.zero;
    }
    else
      go.transform.DOScale(Vector3.zero, PANEL_TWEEN_DURATION)
        .SetUpdate(UpdateType.Normal, independent);
  }

  private void SelectFirstButton()
  {
    foreach (
      var button in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    )
    {
      if (!button.interactable)
        continue;
      EventSystem.current.SetSelectedGameObject(button.gameObject);
      break;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // HUD do Jogador (Enable / Disable)
  // ═══════════════════════════════════════════════════════════════════════════

  private void DisableHud(int playerID)
  {
    HidePanel(
      HudPanelType.AmethystCounter,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(HudPanelType.HealthBar, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.BoostBar, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.DashIcon, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.Score, playerID, independent: true, fade: false, instant: true);
    HidePanel(HudPanelType.Stopwatch, playerID, independent: true, fade: false, instant: true);
  }

  private void EnableHUD(int playerID)
  {
    ShowPanel(HudPanelType.AmethystCounter, playerID, independent: true, fade: false);
    ShowPanel(HudPanelType.HealthBar, playerID, independent: true, fade: false);
    ShowPanel(HudPanelType.BoostBar, playerID, independent: true, fade: false);
    ShowPanel(HudPanelType.DashIcon, playerID, independent: true, fade: false);
    ShowPanel(HudPanelType.Score, playerID, independent: true, fade: false);
    ShowPanel(HudPanelType.Stopwatch, playerID, independent: true, fade: false);
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Shake de Câmera (agora via Awaitable, sem Coroutines)
  // ═══════════════════════════════════════════════════════════════════════════

  public void CameraShake(int playerID, float amplitude, float frequency, float duration = 0f)
  {
    if (!_playerNoises.TryGetValue(playerID, out var noise))
      return;

    CancelPendingShakeStop(playerID);

    noise.AmplitudeGain = amplitude;
    noise.FrequencyGain = frequency;

    if (duration > 0f)
      ScheduleShakeStop(playerID, noise, duration);
  }

  public void RunningShake(int playerID, bool active)
  {
    if (active)
    {
      CameraShake(playerID, Running.Amplitude, Running.Frequency);
      return;
    }

    if (!_playerNoises.TryGetValue(playerID, out var noise))
      return;

    CancelPendingShakeStop(playerID);
    ScheduleShakeStop(playerID, noise, Running.StopDelay);
  }

  private void ScheduleShakeStop(
    int playerID,
    CinemachineBasicMultiChannelPerlin noise,
    float delay
  )
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
    _shakeCts[playerID] = cts;

    // Fire-and-forget: o Awaitable inicia imediatamente, como uma coroutine,
    // mas é cancelável via token em vez de StopCoroutine.
    StopShakingAfterAsync(playerID, noise, delay, cts.Token);
  }

  private void CancelPendingShakeStop(int playerID)
  {
    if (!_shakeCts.TryGetValue(playerID, out var existing) || existing == null)
      return;

    existing.Cancel();
    existing.Dispose();
    _shakeCts[playerID] = null;
  }

  private async Awaitable StopShakingAfterAsync(
    int playerID,
    CinemachineBasicMultiChannelPerlin noise,
    float delay,
    CancellationToken token
  )
  {
    try
    {
      await Awaitable.WaitForSecondsAsync(delay, token);
    }
    catch (OperationCanceledException)
    {
      return;
    }

    noise.AmplitudeGain = 0f;
    noise.FrequencyGain = 0f;
    _shakeCts[playerID] = null;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Popup de Interação
  // ═══════════════════════════════════════════════════════════════════════════

  public void InteractionPopup(int playerID, bool seeing, InteractableObject obj)
  {
    if (!interactionTexts.ContainsKey(playerID) || !interactionImages.ContainsKey(playerID))
      return;

    var text = interactionTexts[playerID];
    var image = interactionImages[playerID];

    if (!seeing)
    {
      HidePanel(HudPanelType.InteractionPopup, playerID, independent: true);
      text.DOColor(Color.white, PANEL_TWEEN_DURATION);
      text.text = string.Empty;
      image.sprite = originalSprites[playerID];
      return;
    }

    string bindLabel = InputSystem.actions.FindAction("Interaction").GetBindingDisplayString();
    ApplyInteractionVisuals(obj, text, image, bindLabel);
    ShowPanel(HudPanelType.InteractionPopup, playerID, independent: true);
  }

  private void ApplyInteractionVisuals(
    InteractableObject obj,
    TextMeshProUGUI text,
    Image image,
    string bindLabel
  )
  {
    switch (obj)
    {
      case PuzzleColorButton pcb:
        text.DOColor(pcb.buttonCode.Color, PANEL_TWEEN_DURATION);
        text.text = bindLabel;
        break;

      case GraplingHookTarget:
        if (GetIcon("GHOOK") is IconImage icon)
          image.sprite = icon.Sprite;
        break;

      default:
        text.text = bindLabel;
        break;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Barras Cinemáticas
  // ═══════════════════════════════════════════════════════════════════════════

  private void TriggerCinematicBars(int playerID, float duration)
  {
    List<GameObject> cinematicPanels = GetCinematicBarPanels(playerID);
    if (cinematicPanels.Count == 0)
      return;

    float halfDuration = duration / 2f;
    AnimateCinematicBars(cinematicPanels, targetSize: 250f, halfDuration);
  }

  private List<GameObject> GetCinematicBarPanels(int playerID)
  {
    var result = new List<GameObject>();
    var holders = GetPanel(playerID, HudPanelType.Cutscene);

    foreach (var holder in holders)
    foreach (var rect in holder.GetComponentsInChildren<RectTransform>(true))
    {
      if (rect.name == "Top" || rect.name == "Bottom")
        result.Add(rect.gameObject);
    }

    return result;
  }

  private static void AnimateCinematicBars(
    List<GameObject> panels,
    float targetSize,
    float duration
  )
  {
    DOTween
      .Sequence()
      .AppendCallback(() => SetBarSize(panels, targetSize, duration))
      .AppendInterval(duration)
      .AppendCallback(() => SetBarSize(panels, 0f, duration));
  }

  private static void SetBarSize(List<GameObject> panels, float height, float duration)
  {
    foreach (var panel in panels)
    {
      if (!panel.TryGetComponent(out RectTransform rect))
        continue;
      rect.DOSizeDelta(new Vector2(rect.rect.width, height), duration).SetEase(Ease.InOutCubic);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Lock On
  // ═══════════════════════════════════════════════════════════════════════════

  private void SetLockOnVisibility(int playerID, bool set, Vector3 position)
  {
    IEnumerable<GameObject> panels;

    if (set)
      panels = ShowPanel(HudPanelType.LockOnOverlay, playerID, independent: false);
    else
    {
      HidePanel(HudPanelType.LockOnOverlay, playerID, independent: false, instant: true);
      panels = GetPanel(playerID, HudPanelType.LockOnOverlay);
    }

    foreach (var go in panels)
    {
      if (go.TryGetComponent(out LockOnOverlay overlay))
        overlay.TargetPosition = position;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Teleporte (agora via Awaitable, sem Coroutines)
  // ═══════════════════════════════════════════════════════════════════════════

  private void TeleportFade(int playerID) => TeleportFadeAsync(playerID, destroyCancellationToken);

  private async Awaitable TeleportFadeAsync(int playerID, CancellationToken token)
  {
    GameObject teleportPanel = GetPanel(playerID, HudPanelType.TeleportFadePanel).FirstOrDefault();

    if (!teleportPanel)
    {
      Debug.LogWarning("[HudDirector] TeleportFadePanel não encontrado para o jogador " + playerID);
      return;
    }

    ShowPanel(HudPanelType.TeleportFadePanel, playerID, independent: false);

    try
    {
      await WaitRealtimeSecondsAsync(TELEPORT_FADE_SECONDS, token);
    }
    catch (OperationCanceledException)
    {
      return;
    }

    HidePanel(HudPanelType.TeleportFadePanel, playerID, independent: false);
  }

  private static async Awaitable WaitRealtimeSecondsAsync(float seconds, CancellationToken token)
  {
    float elapsed = 0f;
    while (elapsed < seconds)
    {
      await Awaitable.NextFrameAsync(token);
      elapsed += Time.unscaledDeltaTime;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Morte e Respawn
  // ═══════════════════════════════════════════════════════════════════════════

  private void DeathPanel()
  {
    CursorOptions(visible: true);
    if (AudioManager.Instance != null && _backgroundMusicConfig != null)
    {
      AudioManager.Instance.PlaySFX(_backgroundMusicConfig.GameOverMusic);
    }

    ForEachPlayer(player =>
    {
      ShowPanel(HudPanelType.GameOver, player.ID, independent: true);
      DisableHud(player.ID);
    });
  }

  private void RespawnPanel()
  {
    CursorOptions(visible: false);
    ForEachPlayer(player =>
    {
      HidePanel(HudPanelType.GameOver, player.ID, independent: true);
      HidePanel(HudPanelType.EndGame, player.ID, independent: true);
      EnableHUD(player.ID);
    });
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Fim de Jogo
  // ═══════════════════════════════════════════════════════════════════════════

  private void EndPanel()
  {
    CursorOptions(visible: true);
    ForEachPlayer(player =>
    {
      DisableHud(player.ID);
      var panels = ShowPanel(HudPanelType.EndGame, player.ID, independent: true);

      if (panels.FirstOrDefault()?.GetComponent<EndGamePanel>() is { } endGamePanel)
      {
        var dataDirector = DataDirector.Instance;
        var levelManager = FindAnyObjectByType<LevelManager>();
        if (dataDirector != null && levelManager != null)
        {
          int currentSlot = dataDirector.GetCurrentSlot();
          int score = player.CurrentScore;
          int previewScore = dataDirector.GetPlayerPreviewScore(
            currentSlot,
            SceneManager.GetActiveScene().name,
            player.ID
          );
          int maxScore = levelManager.ReferenceScore;
          float time = _playerCachedStopwatches.TryGetValue(player.ID, out var sw)
            ? sw.Elapsed
            : 0f;
          ;

          endGamePanel.Populate(
            score,
            previewScore,
            time,
            EndGamePanel.CalculateRank(score, maxScore)
          );
        }
      }
    });
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Combo
  // ═══════════════════════════════════════════════════════════════════════════

  private void ComboPanel(int playerID, int comboIndex, ComboPopupType comboPopupType)
  {
    if (comboIndex < 0)
    {
      HidePanel(HudPanelType.Combo, playerID, independent: true);
      return;
    }

    var panels = GetPanel(playerID, HudPanelType.Combo);
    if (panels.Count == 0)
      return;

    if (panels[0].transform.localScale != Vector3.one)
      ShowPanel(HudPanelType.Combo, playerID, independent: true);

    UpdateComboText(panels, comboIndex);
    TryShowComboPopup(playerID, comboPopupType);
  }

  private void UpdateComboText(List<GameObject> panels, int comboIndex)
  {
    foreach (var go in panels)
    foreach (var text in go.GetComponentsInChildren<TextMeshProUGUI>())
    {
      if (!text.name.Contains("Output", StringComparison.OrdinalIgnoreCase))
        continue;

      text.text = $"x{comboIndex + 1}";
      text.transform.DOKill();
      text.transform.localScale = Vector3.one;
      text.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, vibrato: 1, elasticity: 0.5f)
        .SetUpdate(UpdateType.Normal, isIndependentUpdate: true);
    }
  }

  private void TryShowComboPopup(int playerID, ComboPopupType comboPopupType)
  {
    if (comboPopupType == ComboPopupType.None)
      return;

    var popupPanels = GetPanel(playerID, HudPanelType.ComboPopup);
    if (popupPanels.Count == 0)
      return;

    var sprite = _popupComboImages.Find(img => img.Type == comboPopupType).Sprite;
    if (sprite == null)
      return;

    if (popupPanels[0].TryGetComponent(out Image image))
      image.sprite = sprite;

    PunchPanelTemporary(HudPanelType.ComboPopup, playerID, _comboPunchSettings);
  }

  private void MaxComboPanel(int playerID, ImpactPopupType impactType)
  {
    GameObject maxComboPanel = GetPanel(playerID, HudPanelType.MaxComboPopup)[0];
    if (maxComboPanel != null && maxComboPanel.TryGetComponent(out Image imageComponent))
    {
      imageComponent.sprite = impactType == ImpactPopupType.Slam ? _slamSprite : _whooshSprite;
      PunchPanelTemporary(HudPanelType.MaxComboPopup, playerID, _maxComboPunchSettings);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Score
  // ═══════════════════════════════════════════════════════════════════════════

  private void ScorePanel(int playerID, int newScore)
  {
    GameObject scorePanel = GetPanel(playerID, HudPanelType.Score)[0];

    int currentScore = _playerCachedScores.ContainsKey(playerID)
      ? _playerCachedScores[playerID]
      : 0;
    _playerCachedScores[playerID] = newScore;

    TextMeshProUGUI textMeshPro = scorePanel.GetComponentInChildren<TextMeshProUGUI>();
    if (textMeshPro != null)
    {
      textMeshPro.DOKill();

      DOVirtual
        .Int(
          from: currentScore,
          to: newScore,
          duration: 0.6f,
          onVirtualUpdate: (value) =>
          {
            textMeshPro.text = value.ToString("D8");
          }
        )
        .SetEase(Ease.OutQuad)
        .SetUpdate(true);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Pausa
  // ═══════════════════════════════════════════════════════════════════════════

  private void PausePanel(bool set)
  {
    CursorOptions(visible: set);
    ForEachPlayer(player =>
    {
      if (set)
      {
        ShowPanel(HudPanelType.Pause, player.ID, independent: true);
        DisableHud(player.ID);
      }
      else
      {
        HidePanel(HudPanelType.Pause, player.ID, independent: true);
        EnableHUD(player.ID);
      }
    });
  }

  private void OptionsPausePanel(bool set) { } // TODO: abrir painel/cena de opções

  private void SoundOptionsPausePanel(bool set) { } // TODO: menu de som

  // ═══════════════════════════════════════════════════════════════════════════
  // Helpers Gerais
  // ═══════════════════════════════════════════════════════════════════════════

  private IconImage? GetIcon(string destiny) => _icons.Find(icon => icon.Destiny == destiny);

  public List<GameObject> GetPanel(int playerID, HudPanelType panel) =>
    canvasMap.TryGetValue(playerID, out var dict) && dict.TryGetValue(panel, out var result)
      ? result
      : new List<GameObject>();

  /// <summary>
  /// Retorna todos os GameObjects filhos (incluindo raiz) de um painel.
  /// </summary>
  public IEnumerable<GameObject> GetPanelObjects(int playerID, HudPanelType panel)
  {
    foreach (var root in GetPanel(playerID, panel))
    foreach (var t in root.GetComponentsInChildren<Transform>(true))
      yield return t.gameObject;
  }
}
