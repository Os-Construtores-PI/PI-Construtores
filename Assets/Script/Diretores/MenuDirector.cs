using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuDirector : MonoBehaviour
{
  [Header("Canvas Raiz")]
  [SerializeField]
  private Transform canvasRoot;

  [SerializeField]
  private GameObject _continueButton;

  [SerializeField]
  private GameObject _newGame;

  [SerializeField]
  private Scrollbar _rankingScrollbar;

  [SerializeField]
  private float panelTransitionDelay = 0.35f;

  private bool _bloquearHoverInicial;
  [SerializeField] private GameObject _ultimoBotaoSelecionado;

  [System.Serializable]
  private struct PanelSelectable
  {
    public MenuPanelTypes panel;
    public GameObject selectable;
  }

  [Header("Objeto Selecionado Padrão por Painel")]
  [Tooltip(
    "Objeto que será selecionado quando o painel terminar de ser exibido/transicionado. Se não for definido, cai no fallback (primeiro botão interagível)."
  )]
  [SerializeField]
  private List<PanelSelectable> _defaultSelectables = new();

  private static readonly Dictionary<MenuPanelTypes, string> PanelNames = new()
  {
    { MenuPanelTypes.Menu, "Menu" },
    { MenuPanelTypes.OptionsMenu, "OptionsMenu" },
    { MenuPanelTypes.AudioMenu, "AudioMenu" },
    { MenuPanelTypes.SaveMenu, "SaveMenu" },
    { MenuPanelTypes.LeaderboardMenu, "LeaderboardMenu" },
  };

  private readonly Dictionary<string, List<GameObject>> panels = new();
  private readonly Dictionary<MenuPanelTypes, GameObject> defaultSelectableMap = new();
  private readonly List<Button> currentButtons = new();
  private readonly Stack<MenuPanelTypes> panelHistory = new();

  private MenuPanelTypes _currentPanel = MenuPanelTypes.None;

  // Painel que está aguardando suas animações de entrada terminarem
  // antes de poder selecionar algo. Enquanto isso for != None,
  // nenhuma seleção "prematura" deve ocorrer para esse painel.
  private MenuPanelTypes _pendingAnimationPanel = MenuPanelTypes.None;

  private EventSystem _eventSystem;
  private int animationsRemaining;
  private bool _loadingGame;

  private void Awake()
  {
    _eventSystem = EventSystem.current;

    Time.timeScale = 1f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    BuildPanelMap();
    BuildDefaultSelectableMap();
  }

  #region Start / Update

  private void Start()
  {
    InitMenu();
    UpdateContinueButton();
  }

  private void Update()
  {
    if (_loadingGame)
      return;

    if (BackPressed())
    {
      HandleBack();
      return;
    }

    if (EventSystem.current.currentSelectedGameObject != null)
      return;

    // Enquanto um painel está aguardando o fim de suas animações,
    // não force nenhuma seleção via input — isso é feito só quando
    // NotifyAnimationsFinished confirmar que a transição acabou.
    if (_pendingAnimationPanel != MenuPanelTypes.None)
      return;

    Gamepad gamepad = Gamepad.current;
    if (
      gamepad != null
      && (gamepad.dpad.ReadValue() != Vector2.zero || gamepad.leftStick.ReadValue() != Vector2.zero)
    )
    {
      SelectFirstButton();
    }

    if (Keyboard.current.anyKey.wasPressedThisFrame)
    {
      SelectFirstButton();
    }

#if UNITY_EDITOR
    if (Keyboard.current.f12Key.wasPressedThisFrame)
    {
      DataDirector.Instance.ClearGameData();
    }
#endif
  }

  private void SelectFirstButton()
  {
    SelectDefaultOrFirstButton(_currentPanel);

    ProcessarHover();
  }

  /// <summary>
  /// Seleciona o objeto configurado como padrão para o painel informado.
  /// Se não houver um objeto padrão válido (nulo, inativo ou não interagível),
  /// cai no comportamento antigo de fallback (botão "Novo Jogo" no Menu, ou
  /// o primeiro botão ativo/interagível da lista).
  /// </summary>
  private void SelectDefaultOrFirstButton(MenuPanelTypes panel)
  {
    if (
      defaultSelectableMap.TryGetValue(panel, out var defaultObj)
      && defaultObj != null
      && defaultObj.activeInHierarchy
    )
    {
      Button defaultButton = defaultObj.GetComponent<Button>();

      if (defaultButton == null || defaultButton.interactable)
      {
        EventSystem.current.SetSelectedGameObject(defaultObj);
        Canvas.ForceUpdateCanvases();
        return;
      }
    }

    if (panel == MenuPanelTypes.Menu && _newGame != null)
    {
      Button newGameButton = _newGame.GetComponent<Button>();

      if (
        newGameButton != null
        && newGameButton.gameObject.activeInHierarchy
        && newGameButton.interactable
      )
      {
        EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        Canvas.ForceUpdateCanvases();
        return;
      }
    }

    foreach (var btn in currentButtons)
    {
      if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable)
      {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
        break;
      }
    }
  }

  private void ProcessarHover()
  {
    if (_eventSystem == null)
      return;

    GameObject selecionado = _eventSystem.currentSelectedGameObject;

    if (selecionado == null)
      return;

    // Durante a seleção inicial do painel,
    // não toca o som.
    if (_bloquearHoverInicial)
    {
      _ultimoBotaoSelecionado = selecionado;
      return;
    }

    // Só toca quando realmente mudou de seleção.
    if (selecionado != _ultimoBotaoSelecionado)
    {
      if (AudioManager.Instance != null)
      {
        // Usa o mesmo Hover configurado no UIAudioConfig.
        // Se Hover for nulo, simplesmente não toca.
        UIAudioConfig audioConfig = FindFirstObjectByType<BasicMenuLogic>()?
            .GetComponent<BasicMenuLogic>() != null
            ? null
            : null;
      }

      _ultimoBotaoSelecionado = selecionado;
    }
  }

  private bool BackPressed()
  {
    bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    bool gamepad = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

    return keyboard || gamepad;
  }

  private void HandleBack()
  {
    if (!MenuSelectable.CanSelect)
      return;

    if (_pendingAnimationPanel != MenuPanelTypes.None)
      return;

    GoBack();
  }

  public void GoBack()
  {
    if (panelHistory.Count == 0)
      return;

    SwitchPanel(_currentPanel, panelHistory.Pop());
  }

  private void InitMenu()
  {
    ShowPanel(MenuPanelTypes.Menu);
  }

  public void UpdateContinueButton()
  {
    if (_continueButton == null)
      return;

    bool checkpoint = DataDirector.Instance.AnySlotHasCheckpoint(out _);
    bool completed = DataDirector.Instance.AnySlotCompleted();

    Debug.Log($"Checkpoint: {checkpoint}");
    Debug.Log($"Completed : {completed}");

    _continueButton.SetActive(checkpoint || completed);
  }

  #endregion

  #region Panel Discovery

  private void BuildPanelMap()
  {
    panels.Clear();

    CollectPanelsRecursive(canvasRoot);
  }

  private void CollectPanelsRecursive(Transform parent)
  {
    foreach (Transform child in parent)
    {
      if (!panels.TryGetValue(child.name, out var list))
      {
        list = new List<GameObject>();
        panels[child.name] = list;
      }

      list.Add(child.gameObject);
      CollectPanelsRecursive(child);
    }
  }

  private void BuildDefaultSelectableMap()
  {
    defaultSelectableMap.Clear();

    foreach (var entry in _defaultSelectables)
    {
      if (entry.selectable != null)
        defaultSelectableMap[entry.panel] = entry.selectable;
    }
  }

  #endregion

  #region Panel API

  private bool TryGetPanelRoots(MenuPanelTypes panel, out List<GameObject> roots)
  {
    roots = null;
    return PanelNames.TryGetValue(panel, out var panelName)
      && panels.TryGetValue(panelName, out roots);
  }

  private void ShowPanel(MenuPanelTypes panel)
  {
    Debug.Log("SHOW PANEL -> " + panel);

    bool isMainMenu = panel == MenuPanelTypes.Menu;

    if (isMainMenu)
    {
      _eventSystem.sendNavigationEvents = false;
      MenuSelectable.CanSelect = false;
      MenuSelectionCursor.Instance.Hide();
    }

    if (!TryGetPanelRoots(panel, out var roots))
      return;

    _currentPanel = panel;
    currentButtons.Clear();
    animationsRemaining = 0;
    _pendingAnimationPanel = MenuPanelTypes.None;

    EventSystem.current.SetSelectedGameObject(null);

    foreach (var root in roots)
    {
      root.SetActive(true);
      root.transform.localScale = Vector3.one;

      foreach (Button btn in root.GetComponentsInChildren<Button>(true))
      {
        btn.interactable = true;
        currentButtons.Add(btn);
      }

      animationsRemaining += root.GetComponentsInChildren<MenuButtonSlide>(false).Length;
    }

    Canvas.ForceUpdateCanvases();

    if (animationsRemaining > 0)
    {
      // Este painel tem animação de entrada (MenuButtonSlide). A seleção
      // só deve acontecer quando a transição estiver 100% completa —
      // isso é feito em NotifyAnimationsFinished, nunca aqui.
      _pendingAnimationPanel = panel;
    }
    else
    {
      // Sem animação de entrada: a transição já está completa agora,
      // então pode selecionar imediatamente.
      SelectDefaultOrFirstButton(panel);
    }

    MenuPreview.Instance.gameObject.SetActive(false);
  }

  private void LockCurrentPanel()
  {
    EventSystem.current.SetSelectedGameObject(null);
    _eventSystem.sendNavigationEvents = false;

    foreach (Button btn in currentButtons)
    {
      if (btn != null)
        btn.interactable = false;
    }

    if (TryGetPanelRoots(_currentPanel, out var roots))
    {
      foreach (var root in roots)
        root.SetActive(false);
    }
  }

  private void HidePanel(MenuPanelTypes panel, System.Action onComplete = null)
  {
    if (!TryGetPanelRoots(panel, out var roots))
    {
      onComplete?.Invoke();
      return;
    }

    EventSystem.current.SetSelectedGameObject(null);

    int remaining = roots.Count;

    foreach (var root in roots)
    {
      root.transform.DOKill();
      root.transform.DOScale(Vector3.zero, .25f)
        .SetLink(root)
        .OnComplete(() =>
        {
          if (root != null)
            root.SetActive(false);

          if (--remaining <= 0)
            onComplete?.Invoke();
        });
    }
  }

  private void SwitchPanel(MenuPanelTypes from, MenuPanelTypes to)
  {
    if (_loadingGame)
      return;

    HidePanel(from, () => ShowPanel(to));
  }

  public void OpenPanel(MenuPanelTypes next)
  {
    if (_loadingGame)
      return;

    panelHistory.Push(_currentPanel);
    SwitchPanel(_currentPanel, next);
  }

  #endregion

  #region Scene / App Control

  public void EnterOptions()
  {
    OpenPanel(MenuPanelTypes.OptionsMenu);
  }

  public void ContinueGame()
  {
    DOTween.KillAll();

    int slot = DataDirector.Instance.GetCurrentSlot();
    string level = DataDirector.Instance.GetLastLevelName(slot);

    if (DataDirector.Instance.IsSlotCompleted(slot))
    {
      if (DataDirector.Instance.AnySlotHasCheckpoint(out SavedSlotData slotData))
      {
        level = slotData.lastLevelName;
      }
      else
      {
        StartNewGamePlus(slot);
        return;
      }
    }

    DataDirector.Instance.SaveHasSave(true);
    DataDirector.Instance.ShowStageIntro = true;

    LockCurrentPanel();

    SceneManager.LoadScene(level);
  }

  public void EnterAudioOptions()
  {
    OpenPanel(MenuPanelTypes.AudioMenu);
  }

  public void EnterSaveMenu()
  {
    OpenPanel(MenuPanelTypes.SaveMenu);
  }

  public void EnterLeaderboardMenu()
  {
    OpenPanel(MenuPanelTypes.LeaderboardMenu);
  }

  public void QuitGame()
  {
    Application.Quit();
#if UNITY_EDITOR
    Debug.Log("[MenuDirector] Quitted Game");
#endif
  }

  public void StartNewGamePlus(int slot)
  {
    DataDirector.Instance.SaveHasSave(false);
    DataDirector.Instance.ShowStageIntro = true;

    if (TryGetPanelRoots(MenuPanelTypes.SaveMenu, out var roots))
    {
      foreach (var root in roots)
        root.SetActive(false);
    }

    SceneManager.LoadScene("Fase0");
  }

  private void OnDestroy()
  {
    foreach (var panel in panels)
    {
      foreach (var obj in panel.Value)
      {
        if (obj != null)
          obj.transform.DOKill();
      }
    }
  }

  public void ForceSelection()
  {
    Debug.Log("FORCE SELECTION");

    EventSystem.current.SetSelectedGameObject(_newGame);

    SelectFirstButton();

    GameObject selected = EventSystem.current.currentSelectedGameObject;

    Debug.Log("Selected = " + (selected == null ? "NULL" : selected.name));

    if (selected == null)
      return;

    Button btn = selected.GetComponent<Button>();

    if (btn != null)
    {
      Debug.Log("SHOW CURSOR");
      MenuSelectionCursor.Instance.ShowAfterAnimation(btn);
    }

    MenuSelectable selectable = selected.GetComponent<MenuSelectable>();

    if (selectable != null)
      selectable.ForcePreview();
  }

  /// <summary>
  /// Versão genérica de ForceSelection para painéis que não são o Menu
  /// principal: seleciona o objeto padrão (ou fallback) do painel e
  /// posiciona o cursor de seleção sobre ele, sem depender de campos
  /// específicos do Menu (como _newGame) ou do MenuPreview.
  /// </summary>
  private void FinishGenericPanelSelection(MenuPanelTypes panel)
  {
    SelectDefaultOrFirstButton(panel);

    GameObject selected = EventSystem.current.currentSelectedGameObject;

    if (selected == null)
      return;

    Button btn = selected.GetComponent<Button>();

    if (btn != null)
      MenuSelectionCursor.Instance.ShowAfterAnimation(btn);

    MenuSelectable selectable = selected.GetComponent<MenuSelectable>();

    if (selectable != null)
      selectable.ForcePreview();
  }

  public void NotifyAnimationsFinished()
  {
    animationsRemaining--;

    Debug.Log("Animations Remaining = " + animationsRemaining);

    if (animationsRemaining > 0)
      return;

    Debug.Log("TODAS TERMINARAM");

    MenuPanelTypes finishedPanel = _pendingAnimationPanel;
    _pendingAnimationPanel = MenuPanelTypes.None;

    MenuSelectable.CanSelect = true;

    MenuSelectable[] buttons = FindObjectsByType<MenuSelectable>(FindObjectsSortMode.None);

    foreach (var b in buttons)
      b.MostrarSprite();

    EnableNavigation();

    if (finishedPanel == MenuPanelTypes.Menu)
    {
      MenuPreview.Instance.gameObject.SetActive(true);
      ForceSelection();
    }
    else if (finishedPanel != MenuPanelTypes.None)
    {
      FinishGenericPanelSelection(finishedPanel);
    }
  }

  public void EnableNavigation()
  {
    _eventSystem.sendNavigationEvents = true;
  }

  #endregion
}
