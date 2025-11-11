using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HudDirector : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime WAITTELEPORTFADE = new(1f);
    private static readonly WaitForSecondsRealtime WAITSHAKECAM = new(.25f);

    [SerializeField] private List<IconImage> icons = new();

    private readonly Dictionary<int, Dictionary<string, List<GameObject>>> canvasMap = new();
    private readonly Dictionary<int, TextMeshProUGUI> interactionTexts = new();
    private readonly Dictionary<int, Image> interactionImages = new();
    private readonly Dictionary<int, Sprite> ogSprites = new();

    #region Unity Events
    private void OnEnable()
    {
        if (!GlobalEventBus.HasInstance) return;
        GlobalEventBus.Instance.OBJECTWASSEEN.AddListener(InteractionPopup);
        GlobalEventBus.Instance.TRIGGEREDCINEMATIC.AddListener(TriggerCinematicBars);
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateAmethysts);
        GlobalEventBus.Instance.TRIGGEREDTELEPORT.AddListener(TeleportFade);
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(DeathPanel);
        GlobalEventBus.Instance.PLAYERTRIGGERREDRESPAWN.AddListener(RespawnPanel);
    }

    private void OnDisable()
    {
        if (!GlobalEventBus.HasInstance) return;

        GlobalEventBus.Instance.OBJECTWASSEEN.RemoveListener(InteractionPopup);
        GlobalEventBus.Instance.TRIGGEREDCINEMATIC.RemoveListener(TriggerCinematicBars);
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateAmethysts);
        GlobalEventBus.Instance.TRIGGEREDTELEPORT.RemoveListener(TeleportFade);
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.RemoveListener(DeathPanel);
        GlobalEventBus.Instance.PLAYERTRIGGERREDRESPAWN.RemoveListener(RespawnPanel);
    }

    private void Start()
    {
        DOTween.Init();
    }
    #endregion

    #region Initialization
    public GameObject InitializeHUD(Player player, Transform hudParent, GameObject hudPrefab)
    {
        if (player == null || hudPrefab == null || hudParent == null) return null;
        int playerID = player.ID;

        GameObject hudInstance = Instantiate(hudPrefab, hudParent);
        hudInstance.name = $"HUD_Player_ID_{playerID}";

        Canvas.ForceUpdateCanvases();

        var panelMap = new Dictionary<string, List<GameObject>>();
        CollectPanelsRecursive(hudInstance.transform, panelMap);
        canvasMap[playerID] = panelMap;

        HealthHUDComponent healthHUD = hudInstance.GetComponentInChildren<HealthHUDComponent>();
        if (healthHUD && healthHUD.HUDType == HealthHUDType.PLAYER)
            healthHUD.BindToPlayer(player);

        if (panelMap.TryGetValue(Constants.PanelNames.InteractionPopup, out var panels) && panels.Count > 0)
        {
            var text = panels[0].GetComponentInChildren<TextMeshProUGUI>();
            var image = panels[0].GetComponent<Image>();

            if (text) interactionTexts[playerID] = text;
            if (image)
            {
                interactionImages[playerID] = image;
                ogSprites[playerID] = image.sprite;
            }
        }

        HidePanel(Constants.PanelNames.GameOver, playerID,independent:true ,fade: true, instant: true);
        HidePanel(Constants.PanelNames.InteractionPopup, playerID,independent:true, instant: true);
        HidePanel(Constants.PanelNames.TeleportFadePanel, playerID,independent:true,false, false);

        return hudInstance;
    }

    private void CollectPanelsRecursive(Transform parent, Dictionary<string, List<GameObject>> map)
    {
        foreach (Transform child in parent)
        {
            if (!map.ContainsKey(child.name)) map[child.name] = new List<GameObject>();
            map[child.name].Add(child.gameObject);
            CollectPanelsRecursive(child, map);
        }
    }
    #endregion

    #region Panel Handling
    private void HidePanel(string panelName, int playerID,bool independent, bool fade = false, bool instant = false)
    {
        foreach (var go in GetPanel(playerID, panelName))
        {
            if (fade && go.TryGetComponent(out Image image))
            {
                if (instant) image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
                else image.DOFade(0f, .25f).SetUpdate(UpdateType.Normal,independent);
            }

            if (instant) go.transform.localScale = Vector3.zero;
            else go.transform.DOScale(Vector3.zero, .25f).SetUpdate(UpdateType.Normal,independent);
        }
    }

    public void ShowPanel(string panelName, int playerID,bool independent ,bool fade = false)
    {
        foreach (var go in GetPanel(playerID, panelName))
        {
            if (fade && go.TryGetComponent(out Image image))
                image.DOFade(.8f, .25f).SetUpdate(UpdateType.Normal,independent);

            go.transform.DOScale(Vector3.one, .25f).SetUpdate(UpdateType.Normal,independent);
        }
    }
    #endregion

    #region Camera Shake
    public void ShakeCamera()
    {
        if (GameObject.FindWithTag("CinemachineCamera1")
            .TryGetComponent<CinemachineBasicMultiChannelPerlin>(out var noise))
        {
            noise.AmplitudeGain = 1;
            StartCoroutine(StopShaking(noise));
        }
    }

    private IEnumerator StopShaking(CinemachineBasicMultiChannelPerlin noise)
    {
        yield return WAITSHAKECAM;
        noise.AmplitudeGain = 0;
    }
    #endregion

    #region Interaction
    public void InteractionPopup(bool seeing, InteractableObject obj, int playerID)
    {
        if (!interactionTexts.ContainsKey(playerID) || !interactionImages.ContainsKey(playerID))
            return;

        var text = interactionTexts[playerID];
        var image = interactionImages[playerID];
        string interactionBind = InputSystem.actions.FindAction("Interaction").GetBindingDisplayString();
        float duration = .25f;

        if (!seeing)
        {
            HidePanel(Constants.PanelNames.InteractionPopup, playerID,independent:false);
            text.DOColor(Color.white, duration);
            text.text = "";
            image.sprite = ogSprites[playerID];
            return;
        }

        switch (obj)
        {
            case PuzzleColorButton pcb:
                text.DOColor(pcb.buttonCode.color, duration);
                text.text = interactionBind;
                break;

            case BasicButton:
                text.text = interactionBind;
                break;

            case GraplingHookTarget:
                if (GetIcon("GHOOK") is IconImage validIcon)
                    image.sprite = validIcon.sprite;
                break;
        }

        ShowPanel(Constants.PanelNames.InteractionPopup, playerID,independent:false);
    }

    private void UpdateAmethysts(int newAmount) { }
    #endregion

    #region Cinematic Bars
    private void TriggerCinematicBars(int playerID, float duration)
    {
        List<GameObject> holders = GetPanel(playerID, Constants.PanelNames.GraplingHookCutscene);
        List<GameObject> cinematicPanels = new();

        foreach (var holder in holders)
        {
            var rects = holder.GetComponentsInChildren<RectTransform>(true);
            foreach (var rect in rects)
            {
                if (rect.name == "Top" || rect.name == "Bottom")
                    cinematicPanels.Add(rect.gameObject);
            }
        }

        if (cinematicPanels.Count == 0) return;

        float halfDuration = duration / 2f;
        AnimateCinematicBars(cinematicPanels, 250f, halfDuration);
    }

    private void AnimateCinematicBars(List<GameObject> panels, float size, float duration)
    {
        DOTween.Sequence()
            .AppendCallback(() => ChangeRectSize(size, panels, duration))
            .AppendInterval(duration)
            .AppendCallback(() => ChangeRectSize(0f, panels, duration));
    }

    private void ChangeRectSize(float size, List<GameObject> panels, float duration)
    {
        foreach (GameObject panel in panels)
        {
            if (!panel.TryGetComponent(out RectTransform rect)) continue;
            rect.DOSizeDelta(new Vector2(rect.rect.width, size), duration).SetEase(Ease.InOutCubic);
        }
    }
    #endregion
    #region Teleport
    private void TeleportFade(int ID)
    {
        StartCoroutine(TeleportFadeRoutine(ID));
    }
    private IEnumerator TeleportFadeRoutine(int playerID)
    {
        // pega o painel preto do HUD do jogador
        GameObject teleportPanel = GetPanel(playerID, Constants.PanelNames.TeleportFadePanel).FirstOrDefault();
        if (!teleportPanel) { Debug.LogWarning("SEM TELEPORT PANEL"); yield break; }

        // fade in
        ShowPanel(Constants.PanelNames.TeleportFadePanel, playerID,false);
        yield return WAITTELEPORTFADE;

        // fade out
        HidePanel(Constants.PanelNames.TeleportFadePanel, playerID,false,false);
    }
    #endregion

    # region === DEATH === 
    private void DeathPanel()
    {
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            ShowPanel(Constants.PanelNames.GameOver, player.ID, true);
            HidePanel(Constants.PanelNames.AmethystCounter, player.ID, true);
            HidePanel(Constants.PanelNames.HealthBar, player.ID, true);
            HidePanel(Constants.PanelNames.DashIcon, player.ID, true);
        }
    }
    private void RespawnPanel()
    {
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            HidePanel(Constants.PanelNames.GameOver, player.ID, true);
            ShowPanel(Constants.PanelNames.AmethystCounter, player.ID, true);
            ShowPanel(Constants.PanelNames.HealthBar, player.ID, true);
            ShowPanel(Constants.PanelNames.DashIcon, player.ID, true);
        }
    }


    # endregion

    #region Helpers
    private IconImage? GetIcon(string destiny) =>
        icons.Find(icon => icon.destiny == destiny);

    private List<GameObject> GetPanel(int playerID, string panelName) =>
        canvasMap.TryGetValue(playerID, out var dict) && dict.TryGetValue(panelName, out var result)
            ? result
            : new List<GameObject>();
    #endregion
}

