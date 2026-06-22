using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Constants.PlayerShakes;

/// <summary>
/// Gerencia todos os elementos visuais do HUD, câmeras e painéis para cada jogador.
/// Responde a eventos globais do jogo (morte, pausa, diálogo, teleporte, etc.).
/// </summary>
public class HudDirector : MonoBehaviour
{
  // ─── Constantes ────────────────────────────────────────────────────────────

  private static readonly WaitForSecondsRealtime WAIT_TELEPORT_FADE = new(1f);

  private const float PANEL_TWEEN_DURATION = 0.25f;
  private const float PANEL_FADE_OPACITY = 0.8f;

  // ─── Campos Serializados ────────────────────────────────────────────────────

  [Header("Icons")]
  [SerializeField]
  private List<IconImage> icons = new();

  [Header("Audio")]
  [SerializeField]
  private somMenu somMenu;

  // ─── Estado Interno ─────────────────────────────────────────────────────────

  /// <summary>Mapa: playerID → (panelName → lista de GameObjects do painel)</summary>
  private readonly Dictionary<int, Dictionary<string, List<GameObject>>> canvasMap = new();
  private readonly Dictionary<int, TextMeshProUGUI> interactionTexts = new();
  private readonly Dictionary<int, Image> interactionImages = new();
  private readonly Dictionary<int, Sprite> originalSprites = new();
  private readonly Dictionary<int, CameraLogic> playerCameras = new();

  private Player _playerHudOwner;

  // <summary> Coroutines e Noises </summary>
  private readonly Dictionary<int, CinemachineBasicMultiChannelPerlin> _playerNoises = new();

  /// <summary>Coroutine de parada de shake por playerID.</summary>
  private readonly Dictionary<int, Coroutine> _shakeStopCoroutines = new();

  // no bloco de estado interno, junto dos outros dicionários
  private readonly Dictionary<int, Coroutine> _tempPanelCoroutines = new();

  // ─── Nomes de painéis válidos ───────────────────────────────────────────────

  private static readonly HashSet<string> ValidPanelNames = new()
  {
    Constants.HudPanelNames.InteractionPopup,
    Constants.HudPanelNames.GameOver,
    Constants.HudPanelNames.EndGame,
    Constants.HudPanelNames.TeleportFadePanel,
    Constants.HudPanelNames.Pause,
    Constants.HudPanelNames.Dialogue,
    Constants.HudPanelNames.AmethystCounter,
    Constants.HudPanelNames.HealthBar,
    Constants.HudPanelNames.DashIcon,
    Constants.HudPanelNames.Cutscene,
    Constants.HudPanelNames.LockOnOverlay,
    Constants.HudPanelNames.BoostBar,
    Constants.HudPanelNames.MaxComboPopup,
    Constants.HudPanelNames.Combo,
    Constants.HudPanelNames.Score,
  };

  // ═══════════════════════════════════════════════════════════════════════════
  // Acesso Público
  // ═══════════════════════════════════════════════════════════════════════════

  public CameraLogic GetCameraScript(int id) => playerCameras[id];

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
    GlobalEventBus.Instance.EndGame.AddListener(EndPanel);
    GlobalEventBus.Instance.LockOnVisibility.AddListener(SetLockOnVisibility);
    GlobalEventBus.Instance.Pause.AddListener(PausePanel);
    GlobalEventBus.Instance.Options.AddListener(OptionsPausePanel);
    GlobalEventBus.Instance.ComboUpdate.AddListener(ComboPanel);
    GlobalEventBus.Instance.MaxComboReached.AddListener(MaxComboPanel);
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
    GlobalEventBus.Instance.EndGame.RemoveListener(EndPanel);
    GlobalEventBus.Instance.LockOnVisibility.RemoveListener(SetLockOnVisibility);
    GlobalEventBus.Instance.Pause.RemoveListener(PausePanel);
    GlobalEventBus.Instance.Options.RemoveListener(OptionsPausePanel);
    GlobalEventBus.Instance.ComboUpdate.RemoveListener(ComboPanel);
    GlobalEventBus.Instance.MaxComboReached.RemoveListener(MaxComboPanel);
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

  /// <summary>
  /// Instancia e configura o HUD para um jogador específico.
  /// </summary>
  public GameObject InitializeHUD(Player player, Transform hudParent, GameObject hudPrefab)
  {
    if (player == null || hudPrefab == null || hudParent == null)
      return null;

    _playerHudOwner = player;
    int playerID = player.ID;

    GameObject hudInstance = Instantiate(hudPrefab, hudParent);
    hudInstance.name = $"HUD_Player_ID_{playerID}";

    Canvas.ForceUpdateCanvases();

    // Mapeia todos os painéis do HUD instanciado
    var panelMap = new Dictionary<string, List<GameObject>>();
    CollectPanelsRecursive(hudInstance.transform, panelMap);
    canvasMap[playerID] = panelMap;

    // Vincula HealthHUD ao jogador
    HealthHUD healthHUD = hudInstance.GetComponentInChildren<HealthHUD>();
    if (healthHUD != null)
      healthHUD.BindToPlayer(player);

    BoostHUD boostHUD = hudInstance.GetComponentInChildren<BoostHUD>();
    if (boostHUD != null)
      boostHUD.BindToPlayer(player);

    // Armazena referências do painel de interação
    if (
      panelMap.TryGetValue(Constants.HudPanelNames.InteractionPopup, out var panels)
      && panels.Count > 0
    )
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
    playerCameras[playerID] = camera;
  }

  public void InitializeNoise(int playerID, CinemachineBasicMultiChannelPerlin noise)
  {
    _playerNoises[playerID] = noise;
  }

  private void HideAllPanels(int playerID)
  {
    HidePanel(
      Constants.HudPanelNames.GameOver,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.EndGame,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.InteractionPopup,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.TeleportFadePanel,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.Pause,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.Dialogue,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.Combo,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.MaxComboPopup,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.Score,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
  }

  /// <summary>
  /// Percorre a hierarquia recursivamente coletando GameObjects com nomes válidos de painel.
  /// </summary>
  private void CollectPanelsRecursive(Transform parent, Dictionary<string, List<GameObject>> map)
  {
    foreach (Transform child in parent)
    {
      if (ValidPanelNames.Contains(child.name))
      {
        if (!map.ContainsKey(child.name))
          map[child.name] = new List<GameObject>();

        map[child.name].Add(child.gameObject);
      }

      CollectPanelsRecursive(child, map);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Controle de Painéis
  // ═══════════════════════════════════════════════════════════════════════════

  private void HidePanel(
    string panelName,
    int playerID,
    bool independent,
    bool fade = false,
    bool instant = false
  )
  {
    foreach (var go in GetPanelObjects(playerID, panelName))
    {
      DisableButton(go);
      ApplyFadeOut(go, fade, instant, independent);
      ApplyScaleOut(go, instant, independent);
    }
  }

  public void ShowPanel(string panelName, int playerID, bool independent, bool fade = false)
  {
    foreach (var go in GetPanelObjects(playerID, panelName))
    {
      EnableButton(go);
      ApplyFadeIn(go, fade, independent);
      go.transform.DOScale(Vector3.one, PANEL_TWEEN_DURATION)
        .SetUpdate(UpdateType.Normal, independent);
    }

    EventSystem.current.SetSelectedGameObject(null);
  }

  private void ShowPanelTemporary(string panelName, int playerID, float duration)
  {
    if (_tempPanelCoroutines.TryGetValue(playerID, out var existing) && existing != null)
      StopCoroutine(existing);

    _tempPanelCoroutines[playerID] = StartCoroutine(
      TemporaryPanelRoutine(panelName, playerID, duration)
    );
  }

  private IEnumerator TemporaryPanelRoutine(string panelName, int playerID, float duration)
  {
    ShowPanel(panelName, playerID, independent: true);
    yield return new WaitForSeconds(duration);
    HidePanel(panelName, playerID, independent: true);
    _tempPanelCoroutines[playerID] = null;
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
      Constants.HudPanelNames.AmethystCounter,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.HealthBar,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.BoostBar,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
    HidePanel(
      Constants.HudPanelNames.DashIcon,
      playerID,
      independent: true,
      fade: false,
      instant: true
    );
  }

  private void EnableHUD(int playerID)
  {
    ShowPanel(Constants.HudPanelNames.AmethystCounter, playerID, independent: true, fade: false);
    ShowPanel(Constants.HudPanelNames.HealthBar, playerID, independent: true, fade: false);
    ShowPanel(Constants.HudPanelNames.BoostBar, playerID, independent: true, fade: false);
    ShowPanel(Constants.HudPanelNames.DashIcon, playerID, independent: true, fade: false);
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Shake de Câmera
  // ═══════════════════════════════════════════════════════════════════════════

  public void CameraShake(int playerID, float amplitude, float frequency, float duration = 0f)
  {
    if (!_playerNoises.TryGetValue(playerID, out var noise))
      return;

    if (_shakeStopCoroutines.TryGetValue(playerID, out var existing) && existing != null)
    {
      StopCoroutine(existing);
      _shakeStopCoroutines[playerID] = null;
    }

    noise.AmplitudeGain = amplitude;
    noise.FrequencyGain = frequency;

    if (duration > 0f)
      _shakeStopCoroutines[playerID] = StartCoroutine(StopShakingAfter(playerID, noise, duration));
  }

  public void RunningShake(int playerID, bool active)
  {
    if (active)
    {
      CameraShake(playerID, Running.Amplitude, Running.Frequency);
    }
    else
    {
      if (!_playerNoises.TryGetValue(playerID, out var noise))
        return;

      if (_shakeStopCoroutines.TryGetValue(playerID, out var existing) && existing != null)
        StopCoroutine(existing);

      _shakeStopCoroutines[playerID] = StartCoroutine(
        StopShakingAfter(playerID, noise, Running.StopDelay)
      );
    }
  }

  private IEnumerator StopShakingAfter(
    int playerID,
    CinemachineBasicMultiChannelPerlin noise,
    float delay
  )
  {
    yield return new WaitForSeconds(delay);
    noise.AmplitudeGain = 0f;
    noise.FrequencyGain = 0f;
    _shakeStopCoroutines[playerID] = null;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Popup de Interação
  // ═══════════════════════════════════════════════════════════════════════════

  public void InteractionPopup(bool seeing, InteractableObject obj, int playerID)
  {
    if (!interactionTexts.ContainsKey(playerID) || !interactionImages.ContainsKey(playerID))
      return;

    var text = interactionTexts[playerID];
    var image = interactionImages[playerID];

    if (!seeing)
    {
      HidePanel(Constants.HudPanelNames.InteractionPopup, playerID, independent: true);
      text.DOColor(Color.white, PANEL_TWEEN_DURATION);
      text.text = string.Empty;
      image.sprite = originalSprites[playerID];
      return;
    }

    string bindLabel = InputSystem.actions.FindAction("Interaction").GetBindingDisplayString();
    ApplyInteractionVisuals(obj, text, image, bindLabel);
    ShowPanel(Constants.HudPanelNames.InteractionPopup, playerID, independent: true);
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
    var holders = GetPanel(playerID, Constants.HudPanelNames.Cutscene);

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
    if (set)
      ShowPanel(Constants.HudPanelNames.LockOnOverlay, playerID, independent: false);
    else
      HidePanel(Constants.HudPanelNames.LockOnOverlay, playerID, independent: false, instant: true);

    foreach (var go in GetPanel(playerID, Constants.HudPanelNames.LockOnOverlay))
    {
      if (go.TryGetComponent(out LockOnOverlay overlay))
        overlay.TargetPosition = position;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Teleporte
  // ═══════════════════════════════════════════════════════════════════════════

  private void TeleportFade(int playerID) => StartCoroutine(TeleportFadeRoutine(playerID));

  private IEnumerator TeleportFadeRoutine(int playerID)
  {
    GameObject teleportPanel = GetPanel(playerID, Constants.HudPanelNames.TeleportFadePanel)
      .FirstOrDefault();

    if (!teleportPanel)
    {
      Debug.LogWarning("[HudDirector] TeleportFadePanel não encontrado para o jogador " + playerID);
      yield break;
    }

    ShowPanel(Constants.HudPanelNames.TeleportFadePanel, playerID, independent: false);
    yield return WAIT_TELEPORT_FADE;
    HidePanel(Constants.HudPanelNames.TeleportFadePanel, playerID, independent: false);
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Morte e Respawn
  // ═══════════════════════════════════════════════════════════════════════════

  private void DeathPanel()
  {
    CursorOptions(visible: true);
    if (AudioManager.Instance != null && somMenu != null)
    {
      AudioManager.Instance.PlaySFX(somMenu.gameOverMenu);
    }

    ForEachPlayer(player =>
    {
      ShowPanel(Constants.HudPanelNames.GameOver, player.ID, independent: true);
      DisableHud(player.ID);
    });
  }

  private void RespawnPanel()
  {
    CursorOptions(visible: false);
    ForEachPlayer(player =>
    {
      HidePanel(Constants.HudPanelNames.GameOver, player.ID, independent: true);
      HidePanel(Constants.HudPanelNames.EndGame, player.ID, independent: true);
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
      ShowPanel(Constants.HudPanelNames.EndGame, player.ID, independent: true);
    });
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // Combo
  // ═══════════════════════════════════════════════════════════════════════════

  private void ComboPanel(int playerID, int comboIndex)
  {
    if (comboIndex < 0)
    {
      HidePanel(Constants.HudPanelNames.Combo, playerID, independent: true);
      return;
    }
    foreach (var go in GetPanel(playerID, Constants.HudPanelNames.Combo))
    {
      foreach (var text in go.GetComponentsInChildren<TextMeshProUGUI>())
      {
        if (text.name.Contains("Output", System.StringComparison.OrdinalIgnoreCase))
          text.text = $"x{comboIndex + 1}";
      }
    }

    ShowPanel(Constants.HudPanelNames.Combo, playerID, independent: true);
  }

  private void MaxComboPanel(int playerID)
  {
    ShowPanelTemporary(Constants.HudPanelNames.MaxComboPopup, playerID, duration: 2f);
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
        ShowPanel(Constants.HudPanelNames.Pause, player.ID, independent: true);
        DisableHud(player.ID);
      }
      else
      {
        HidePanel(Constants.HudPanelNames.Pause, player.ID, independent: true);
        EnableHUD(player.ID);
      }
    });
  }

  private void OptionsPausePanel(bool set) { } // TODO: abrir painel/cena de opções

  private void SoundOptionsPausePanel(bool set) { } // TODO: menu de som

  // ═══════════════════════════════════════════════════════════════════════════
  // Helpers Gerais
  // ═══════════════════════════════════════════════════════════════════════════

  private IconImage? GetIcon(string destiny) => icons.Find(icon => icon.Destiny == destiny);

  private List<GameObject> GetPanel(int playerID, string panelName) =>
    canvasMap.TryGetValue(playerID, out var dict) && dict.TryGetValue(panelName, out var result)
      ? result
      : new List<GameObject>();

  /// <summary>
  /// Retorna todos os GameObjects filhos (incluindo raiz) de um painel.
  /// </summary>
  private IEnumerable<GameObject> GetPanelObjects(int playerID, string panelName)
  {
    foreach (var root in GetPanel(playerID, panelName))
    foreach (var t in root.GetComponentsInChildren<Transform>(true))
      yield return t.gameObject;
  }

  private static void CursorOptions(bool visible)
  {
    Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    Cursor.visible = visible;
  }

  /// <summary>
  /// Itera sobre todos os Players ativos na cena e executa uma ação.
  /// </summary>
  private static void ForEachPlayer(System.Action<Player> action)
  {
    foreach (
      var player in FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    )
      action(player);
  }
}
