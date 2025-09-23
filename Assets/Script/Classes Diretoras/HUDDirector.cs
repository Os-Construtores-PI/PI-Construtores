using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDDirector : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime Wait025 = new(.25f);

    [SerializeField] private List<IconImage> icons = new();

    private readonly Dictionary<int, Dictionary<string, List<GameObject>>> canvasMap = new();
    private readonly Dictionary<int, TextMeshProUGUI> interactionTexts = new();
    private readonly Dictionary<int, Image> interactionImages = new();
    private readonly Dictionary<int, Sprite> ogSprites = new();

    #region Unity Events
    private void OnEnable()
    {
        if (!GlobalEventBus.HasInstance) return;

        GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
        GlobalEventBus.Instance.TriggeredCinematic.AddListener(TriggerCinematicBars);
        GlobalEventBus.Instance.AmethystsAmountChanged.AddListener(UpdateAmethysts);
    }

    private void OnDisable()
    {
        if (!GlobalEventBus.HasInstance) return;

        GlobalEventBus.Instance.ObjectWasSeen.RemoveListener(InteractionPopup);
        GlobalEventBus.Instance.TriggeredCinematic.RemoveListener(TriggerCinematicBars);
        GlobalEventBus.Instance.AmethystsAmountChanged.RemoveListener(UpdateAmethysts);
    }

    private void Start()
    {
        DOTween.Init();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Inicializa o HUD para o player instanciado.
    /// O hudPrefab será instanciado como filho de hudParent.
    /// </summary>
    public void InitializeHUD(Player player, Transform hudParent, GameObject hudPrefab)
    {
        if (player == null || hudPrefab == null || hudParent == null) return;
        int playerID = player.ID;

        // Instancia HUD
        GameObject hudInstance = Instantiate(hudPrefab, hudParent);
        hudInstance.name = $"HUD_Player_ID_{playerID}";

        // Força atualização de layout
        Canvas.ForceUpdateCanvases();

        // Descobre painéis automaticamente
        var panelMap = new Dictionary<string, List<GameObject>>();
        CollectPanelsRecursive(hudInstance.transform, panelMap);
        canvasMap[playerID] = panelMap;

        // Inicia Barra de Vida
        HealthHUDComponent healthHUD = hudInstance.GetComponentInChildren<HealthHUDComponent>();
        if (healthHUD && healthHUD.HUDType == HealthHUDType.PLAYER)
            healthHUD.BindToPlayer(player);

        // Inicializa interação
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

        // Desativa painéis iniciais
        HidePanel(Constants.PanelNames.GameOver, playerID, fade: true, instant: true);
        HidePanel(Constants.PanelNames.InteractionPopup, playerID, instant: true);
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
    private void HidePanel(string panelName, int playerID, bool fade = false, bool instant = false)
    {
        foreach (var go in GetPanel(playerID, panelName))
        {
            if (fade && go.TryGetComponent(out Image image))
            {
                if (instant) image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
                else image.DOFade(0f, .25f);
            }

            if (instant) go.transform.localScale = Vector3.zero;
            else go.transform.DOScale(Vector3.zero, .25f);
        }
    }

    public void ShowPanel(string panelName, int playerID, bool fade = false)
    {
        foreach (var go in GetPanel(playerID, panelName))
        {
            if (fade && go.TryGetComponent(out Image image))
                image.DOFade(.8f, .25f);

            go.transform.DOScale(Vector3.one, .25f);
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
        yield return Wait025;
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
            HidePanel(Constants.PanelNames.InteractionPopup, playerID);
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

        ShowPanel(Constants.PanelNames.InteractionPopup, playerID);
    }

    private void UpdateAmethysts(int newAmount) { }
    #endregion

    #region Cinematic Bars
    private void TriggerCinematicBars(int playerID, float duration)
    {
        // pega o holder
        List<GameObject> holders = GetPanel(playerID, Constants.PanelNames.GraplingHookCutscene);
        List<GameObject> cinematicPanels = new();

        // procura Top e Bottom dentro do holder
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
    public void SetupStartOnly()
    {
        // Percorre todos os painéis do prefab HUD
        foreach (Transform child in transform)
        {
            // Mantém apenas o painel de Start (nomeie corretamente no prefab, ex: "StartPanel")
            if (child.name != "StartPanel")
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
        }
    }

    #region Helpers
    private IconImage? GetIcon(string destiny) =>
        icons.Find(icon => icon.destiny == destiny);

    private List<GameObject> GetPanel(int playerID, string panelName) =>
        canvasMap.TryGetValue(playerID, out var dict) && dict.TryGetValue(panelName, out var result)
            ? result
            : new List<GameObject>();
    #endregion
}
