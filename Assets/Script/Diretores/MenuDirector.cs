using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuDirector : MonoBehaviour
{
  [Header("Canvas Roots")]
  [SerializeField]
  private Transform[] canvasRoots;

  [SerializeField]
  private GameObject _continueButton;

  private string _currentPanel;

  private List<Button> currentButtons = new();

  private readonly Dictionary<string, List<GameObject>> panels = new();

  [SerializeField]
  private LoadingScreen _loadingScreen;

  private EventSystem _eventSystem;

  [SerializeField] private float panelTransitionDelay = 0.35f;

  private void Awake()
  {
    _eventSystem = EventSystem.current;

    Time.timeScale = 1f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    BuildPanelMap();
  }

  #region Start

  private void Start()
  {
    InitMenu();
    UpdateContinueButton();
  }

  private void Update()
  {
    if (EventSystem.current.currentSelectedGameObject != null)
      return;

    if (Gamepad.current != null)
    {
      if (
        Gamepad.current.dpad.ReadValue() != Vector2.zero
        || Gamepad.current.leftStick.ReadValue() != Vector2.zero
      )
      {
        SelectFirstButton();
      }
    }

    if (Keyboard.current.anyKey.wasPressedThisFrame)
    {
      SelectFirstButton();
    }

    if (Input.GetKeyDown(KeyCode.F12))
    {
      DataDirector.Instance.ClearGameData();
    }
  }

  private void SelectFirstButton()
  {

    if (currentButtons.Count == 0)
      return;

    foreach (var btn in currentButtons)
    {
      if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable)
      {
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
        break;
      }
    }
  }

  private void InitMenu()
  {
    ShowPanel(Constants.MenuPanelNames.Menu);
  }

  public void UpdateContinueButton()
  {
    if (_continueButton == null)
      return;

    bool show =
      DataDirector.Instance.AnySlotHasCheckpoint(out _) || DataDirector.Instance.AnySlotCompleted();

    _continueButton.SetActive(show);
  }

  #endregion


  #region Panel Discovery

  private void BuildPanelMap()
  {
    panels.Clear();

    foreach (var root in canvasRoots)
      CollectPanelsRecursive(root);
  }

  private void CollectPanelsRecursive(Transform parent)
  {
    foreach (Transform child in parent)
    {
      if (!panels.ContainsKey(child.name))
        panels[child.name] = new List<GameObject>();

      panels[child.name].Add(child.gameObject);
      CollectPanelsRecursive(child);
    }
  }

  #endregion

  #region Public Panel API

  private void ShowPanel(string panelName, bool fade = false)
  {
    bool animatedMenu = panelName == Constants.MenuPanelNames.Menu;

    if (animatedMenu)
    {
        _eventSystem.sendNavigationEvents = false;
        MenuSelectable.CanSeletc = false;
        MenuSelectionCursor.Instance.Hide();
    }


    if (!panels.TryGetValue(panelName, out var roots))
        return;

    _currentPanel = panelName;
    currentButtons.Clear();

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
}

     Canvas.ForceUpdateCanvases();

     SelectFirstButton();

     MenuPreview.Instance.gameObject.SetActive(false);


}

  private void HidePanel(string panelName, bool fade = false)
  {
    if (!panels.TryGetValue(panelName, out var roots))
      return;

    EventSystem.current.SetSelectedGameObject(null);

    foreach (var root in roots)
    {
      root.transform.DOKill();

      root.transform.DOScale(Vector3.zero, .25f).SetLink(root).OnComplete(() =>
      {
        if (root != null)
          root.SetActive(false);
      });
    }
  }

  private void SwitchPanel(string from, string to, bool fade = false)
  {
    HidePanel(from, fade);

    DOVirtual.DelayedCall(panelTransitionDelay, () =>
    {
      ShowPanel(to, fade);
    });
    //ShowPanel(to, fade);
  }

  #endregion

  #region Scene / App Control

  public void EnterOptions()
  {
    SwitchPanel(Constants.MenuPanelNames.Menu, Constants.MenuPanelNames.OptionsMenu);
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
    _loadingScreen.LoadScene(level);
  }

  public void ExitOptions()
  {
    SwitchPanel(Constants.MenuPanelNames.OptionsMenu, Constants.MenuPanelNames.Menu);
  }

  public void EnterAudioOptions()
  {
    SwitchPanel(Constants.MenuPanelNames.OptionsMenu, Constants.MenuPanelNames.AudioMenu);
  }

  public void ExitAudioOption()
  {
    SwitchPanel(Constants.MenuPanelNames.AudioMenu, Constants.MenuPanelNames.OptionsMenu);
  }

  public void EnterSaveMenu()
  {
    SwitchPanel(Constants.MenuPanelNames.Menu, Constants.MenuPanelNames.SaveMenu);
  }

  public void ExitSaveMenu()
  {
    SwitchPanel(Constants.MenuPanelNames.SaveMenu, Constants.MenuPanelNames.Menu);
  }

  public void LoadScene(string sceneName)
  {

    _loadingScreen.LoadScene(sceneName);
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
    _loadingScreen.LoadScene(Constants.SceneNames.FirstLevel);
  }

  private void OnDestroy()
  {

    foreach (var panel in panels)
    {
      foreach (var obj in panel.Value)
      {
        if(obj != null)
          obj.transform.DOKill();
      }
    }
  }

  public void ForceSelection()
  {
    EventSystem.current.SetSelectedGameObject(null);

    SelectFirstButton();
    
    GameObject selected = EventSystem.current.currentSelectedGameObject;

    if (selected == null)
      return;

    Button btn = selected.GetComponent<Button>();

    if(btn != null)
      MenuSelectionCursor.Instance.ShowAfterAnimation(btn);

    MenuSelectable selectable = selected.GetComponent<MenuSelectable>();

    if(selectable != null)
       selectable.ForcePreview();
  }

  public void EnableNavigation()
{
    _eventSystem.sendNavigationEvents = true;
}

  #endregion

}

