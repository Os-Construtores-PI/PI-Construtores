using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

        // SCANNER OBJETOS
        GlobalEventBus.Instance.OBJECTWASSEEN.AddListener(InteractionPopup);

        // CINEMÁTICA
        GlobalEventBus.Instance.PLAYERTRIGGEREDCINEMATIC.AddListener(TriggerCinematicBars);

        // SISTEMA MONETÁRIO
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateAmethysts);

        //TELEPORTE
        GlobalEventBus.Instance.PLAYERTRIGGEREDTELEPORT.AddListener(TeleportFade);

        // VIDA
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(DeathPanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.AddListener(RespawnPanel);

        // ENDGAME
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.AddListener(EndPanel);

        // PAUSE
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.AddListener(PausePanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDOPTIONS.AddListener(OptionsPausePanel);

        // DIALOGUE
        GlobalEventBus.Instance.PLAYERTRIGGEREDDIALOGUE.AddListener(DialoguePanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDSKIPDIALOGUE.AddListener(SkipText);
    }

    private void OnDisable()
    {
        if (!GlobalEventBus.HasInstance) return;

        GlobalEventBus.Instance.OBJECTWASSEEN.RemoveListener(InteractionPopup);
        GlobalEventBus.Instance.PLAYERTRIGGEREDCINEMATIC.RemoveListener(TriggerCinematicBars);
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateAmethysts);
        GlobalEventBus.Instance.PLAYERTRIGGEREDTELEPORT.RemoveListener(TeleportFade);
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.RemoveListener(DeathPanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.RemoveListener(RespawnPanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.RemoveListener(EndPanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.RemoveListener(PausePanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDDIALOGUE.RemoveListener(DialoguePanel);
        GlobalEventBus.Instance.PLAYERTRIGGEREDSKIPDIALOGUE.RemoveListener(SkipText);
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
        
        HideAllPanels(playerID);
        return hudInstance;
    }

    private void HideAllPanels(int playerID)
    {
        HidePanel(Constants.PanelNames.GameOver, playerID,independent:true ,fade: true, instant: true);
        HidePanel(Constants.PanelNames.EndGame, playerID, independent:true, fade: true, instant: true);
        HidePanel(Constants.PanelNames.InteractionPopup, playerID,independent:true, instant: true);
        HidePanel(Constants.PanelNames.TeleportFadePanel, playerID,independent:true,false, true);        
        HidePanel(Constants.PanelNames.Pause, playerID, independent:true,false,true);
        HidePanel(Constants.PanelNames.Dialogue, playerID, independent:true,fade:true,instant:true);
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
            {
                image.DOFade(.8f, .25f).SetUpdate(UpdateType.Normal,independent);
            }

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
        CursorOptions(true);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            ShowPanel(Constants.PanelNames.GameOver, player.ID, true);
            DisableHud(player.ID);
        }
    }
    private void RespawnPanel()
    {
        CursorOptions(false);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            HidePanel(Constants.PanelNames.GameOver, player.ID, true);
            HidePanel(Constants.PanelNames.EndGame, player.ID, true);
            EnableHUD(player.ID);
        }
    }
    # endregion

    # region === END GAME ===
    private void EndPanel()
    {
        CursorOptions(false);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            ShowPanel(Constants.PanelNames.EndGame, player.ID, true);
            DisableHud(player.ID);    
        }
    }
    # endregion === END GAME ===

    # region === DIALOGUE ===
    private void DialoguePanel(PlayerContext context, List<string> text, float typeSpeed)
    {
        int playerID = context.EntityID;
        CursorOptions(false);
        ShowPanel(Constants.PanelNames.Dialogue, playerID, true);
        DisableHud(playerID);
        GlobalEventBus.Instance.PLAYERTRIGGEREDLOCKDIALOGUE.Invoke(context, false);

        TextMeshProUGUI targetText = GetPanel(playerID,Constants.PanelNames.Dialogue)[0].GetComponentInChildren<TextMeshProUGUI>();
        WriteText(context,targetText,text,typeSpeed);
    }
    private void EndDialoguePanel(PlayerContext context)
    {
        int playerID = context.EntityID;
        CursorOptions(true);
        HidePanel(Constants.PanelNames.Dialogue,playerID,true);
        EnableHUD(playerID);


        GlobalEventBus.Instance.PLAYERTRIGGEREDLOCKDIALOGUE.Invoke(context, true);
    }

    # endregion === DIALOGUE ===

    # region === PAUSE ===
    private void PausePanel(bool set)
    {
        CursorOptions(set);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            if(set)
            {
                ShowPanel(Constants.PanelNames.Pause, player.ID, true);
                DisableHud(player.ID);
            }
            else
            {
                HidePanel(Constants.PanelNames.Pause, player.ID, true);
                EnableHUD(player.ID);
            }
        }
    }
    private void OptionsPausePanel(bool set)
    {
        // aqui você pode abrir outro painel ou cena de opções
    }
    private void SoundOptionsPausePanel(bool set)
    {
        // menu de som
    }
    
    #endregion

    #region Helpers
    private IconImage? GetIcon(string destiny) =>
        icons.Find(icon => icon.destiny == destiny);

    private List<GameObject> GetPanel(int playerID, string panelName) =>
        canvasMap.TryGetValue(playerID, out var dict) && dict.TryGetValue(panelName, out var result)
            ? result
            : new List<GameObject>();


    private void DisableHud(int playerID)
    {
        HidePanel(Constants.PanelNames.AmethystCounter, playerID,true,false,true);
        HidePanel(Constants.PanelNames.HealthBar, playerID, true,false,true);
        HidePanel(Constants.PanelNames.DashIcon, playerID, true,false,true);        
    }
    private void EnableHUD(int playerID)
    {
        ShowPanel(Constants.PanelNames.AmethystCounter, playerID,true,false);
        ShowPanel(Constants.PanelNames.HealthBar, playerID, true,false);
        ShowPanel(Constants.PanelNames.DashIcon, playerID, true,false);    
    }

    private void CursorOptions(bool set)
    {
        Cursor.lockState = set ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = set;  
    }

    private readonly Dictionary<int, Sequence> currentTyping = new();
    private readonly Dictionary<int, int> dialogueIndex = new();
    private readonly Dictionary<int, List<string>> dialogueCache = new();
    private readonly Dictionary<int, TextMeshProUGUI> textTargets = new();
    private readonly Dictionary<int, float> typeSpeeds = new();

    public void WriteText(PlayerContext context, TextMeshProUGUI textTarget, List<string> texts, float typeSpeed)
    {
        int id = context.EntityID;

        // salva dados para skip
        dialogueCache[id] = texts;
        textTargets[id] = textTarget;
        typeSpeeds[id] = typeSpeed;
        dialogueIndex[id] = 0;

        StartLine(context);
    }

    private void StartLine(PlayerContext context)
    {
        int id = context.EntityID;

        // mata tween anterior
        if (currentTyping.TryGetValue(id, out var oldSeq))
            oldSeq.Kill();

        var texts = dialogueCache[id];
        int index = dialogueIndex[id];

        // terminou tudo
        if (index >= texts.Count)
        {
            EndDialoguePanel(context);
            return;
        }

        var text = texts[index];
        var target = textTargets[id];
        float speed = typeSpeeds[id];

        target.text = "";

        Sequence seq = DOTween.Sequence();
        currentTyping[id] = seq;

        seq.Append(
            DOTween.To(() => target.text,
                    x => target.text = x,
                    text,
                    text.Length / speed)
                .SetEase(Ease.Linear)
        );

        seq.OnComplete(() =>
        {
            // espera 1 seg depois da fala completa
            DOVirtual.DelayedCall(1f, () =>
            {
                dialogueIndex[id]++;
                StartLine(context);
            });
        });
    }

    public void SkipText(PlayerContext context)
    {
        int id = context.EntityID;

        if (!currentTyping.TryGetValue(id, out var seq))
            return;

        seq.Complete(true);  // finaliza apenas a fala atual sem callbacks antigos
    }



    private void SetText(TextMeshProUGUI textTarget, string text)
    {
        textTarget.text = text;
    }

    #endregion
}

