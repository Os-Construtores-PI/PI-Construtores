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
  private float panelTransitionDelay = 0.35f;

  private static readonly Dictionary<MenuPanelTypes, string> PanelNames = new()
  {
    { MenuPanelTypes.Menu, "Menu" },
    { MenuPanelTypes.OptionsMenu, "OptionsMenu" },
    { MenuPanelTypes.AudioMenu, "AudioMenu" },
    { MenuPanelTypes.SaveMenu, "SaveMenu" },
    { MenuPanelTypes.LeaderboardMenu, "LeaderboardMenu" },
  };

  private readonly Dictionary<string, List<GameObject>> panels = new();
  private readonly List<Button> currentButtons = new();
  private readonly Stack<MenuPanelTypes> panelHistory = new();

  private MenuPanelTypes _currentPanel = MenuPanelTypes.None;

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
    if (_currentPanel == MenuPanelTypes.Menu && _newGame != null)
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

    bool animatedMenu = panel == MenuPanelTypes.Menu;

    if (animatedMenu)
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

    SelectFirstButton();

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

  public void NotifyAnimationsFinished()
  {
    animationsRemaining--;

    Debug.Log("Animations Remaining = " + animationsRemaining);

    if (animationsRemaining > 0)
      return;

    Debug.Log("TODAS TERMINARAM");

    MenuSelectable.CanSelect = true;

    MenuSelectable[] buttons = FindObjectsByType<MenuSelectable>(FindObjectsSortMode.None);

    foreach (var b in buttons)
      b.MostrarSprite();

    MenuPreview.Instance.gameObject.SetActive(true);

    EnableNavigation();

    ForceSelection();
  }

  public void EnableNavigation()
  {
    _eventSystem.sendNavigationEvents = true;
  }

  #endregion
}
