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
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime_25 = new(.25f);

    [SerializeField] private List<IconImage> icons = new();

    private Dictionary<int, Dictionary<string, List<GameObject>>> canvasMap = new();
    private Dictionary<int, TextMeshProUGUI> interactionTexts = new();
    private Dictionary<int, Image> interactionImages = new();
    private Dictionary<int, Sprite> ogSprites = new();

    private void OnEnable()
    {
        GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
        GlobalEventBus.Instance.TriggeredCinematic.AddListener(TriggerCinematicBars);
    }

    private void OnDisable()
    {
        if (GlobalEventBus.HasInstance)
        {
            GlobalEventBus.Instance.ObjectWasSeen.RemoveListener(InteractionPopup);
            GlobalEventBus.Instance.TriggeredCinematic.RemoveListener(TriggerCinematicBars);
        }
    }

    private void Start()
    {
        DOTween.Init();
    }

    /// <summary>
    /// Inicializa o HUD para o player instanciado.
    /// O hudPrefab será instanciado como filho de hudParent.
    /// </summary>
    public void InitializeHUD(Player player, Transform hudParent, GameObject hudPrefab)
    {
        if (player == null || hudPrefab == null || hudParent == null) return;
        int playerID = player.ID;

        // Instancia o HUD como filho do canvas
        GameObject hudInstance = Instantiate(hudPrefab, hudParent);
        hudInstance.name = $"HUD_Player_ID_{playerID}";

        // Força atualização do layout antes de mexer nos scales
        Canvas.ForceUpdateCanvases();

        // Descobre todos os painéis do HUD automaticamente (recursivamente)
        Dictionary<string, List<GameObject>> panelMap = new();
        CollectPanelsRecursive(hudInstance.transform, panelMap);
        canvasMap[playerID] = panelMap;

        // Iniciar a Barra de Vida
        HealthHUDComponent healthHUD = hudInstance.GetComponentInChildren<HealthHUDComponent>();
        if (healthHUD != null && player != null && healthHUD.HUDType == HealthHUDType.PLAYER)
        {
            healthHUD.BindToPlayer(player);
        }

        // Inicializa textos e imagens de interação se existir
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

        // Desativa painéis iniciais imediatamente
        DisablePanelWithImage(Constants.PanelNames.GameOver, 0f, playerID);
        DisablePanel(Constants.PanelNames.InteractionPopup, 0f, playerID);
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

    // --- Métodos de mostrar/esconder painéis ---
    private void DisablePanelWithImage(string panel_name, float duration, int playerID)
    {
        if (canvasMap.TryGetValue(playerID, out var panelMap) &&
            panelMap.TryGetValue(panel_name, out var panel))
        {
            foreach (var go in panel)
            {
                if (go.TryGetComponent(out Image image))
                {
                    if (duration > 0f)
                        image.DOFade(0f, duration);
                    else
                        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
                }

                if (duration > 0f)
                    go.transform.DOScale(Vector3.zero, duration);
                else
                    go.transform.localScale = Vector3.zero;
            }
        }
    }

    private void DisablePanel(string panel_name, float duration, int playerID)
    {
        if (canvasMap.TryGetValue(playerID, out var panelMap) &&
            panelMap.TryGetValue(panel_name, out var panel))
        {
            foreach (var go in panel)
            {
                if (duration > 0f)
                    go.transform.DOScale(Vector3.zero, duration);
                else
                    go.transform.localScale = Vector3.zero;
            }
        }
    }

    public void ShowFade(string panel_name, int playerID)
    {
        if (canvasMap.TryGetValue(playerID, out var panelMap) &&
            panelMap.TryGetValue(panel_name, out var panels))
        {
            foreach (var go in panels)
            {
                if (go.TryGetComponent(out Image image))
                    image.DOFade(.8f, .25f);
                go.transform.DOScale(Vector3.one, 0.25f);
            }
        }
    }

    public void Show(string panel_name, int playerID)
    {
        if (canvasMap.TryGetValue(playerID, out var panelMap) &&
            panelMap.TryGetValue(panel_name, out var panels))
        {
            foreach (var go in panels)
                go.transform.DOScale(Vector3.one, 0.25f);
        }
    }

    // --- Shake de câmera ---
    public void ShakeCamera()
    {
        if (GameObject.FindWithTag("CinemachineCamera1").TryGetComponent<CinemachineBasicMultiChannelPerlin>(out var noisecomp))
        {
            noisecomp.AmplitudeGain = 1;
            StartCoroutine(StopShaking(noisecomp));
        }
    }

    IEnumerator StopShaking(CinemachineBasicMultiChannelPerlin noise)
    {
        yield return _waitForSecondsRealtime_25;
        noise.AmplitudeGain = 0;
    }

    // --- Popups de interação ---
    public void InteractionPopup(bool seeing, InteractableObject obj, int playerID)
    {
        float durationexpected = .25f;
        string interactionBind = InputSystem.actions.FindAction("Interaction").GetBindingDisplayString();

        if (!interactionTexts.ContainsKey(playerID) || !interactionImages.ContainsKey(playerID))
            return;

        var text = interactionTexts[playerID];
        var image = interactionImages[playerID];

        if (!seeing)
        {
            DisablePanel(Constants.PanelNames.InteractionPopup, durationexpected, playerID);
            text.DOColor(Color.white, durationexpected);
            text.text = "";
            image.sprite = ogSprites[playerID];
            return;
        }

        if (obj is PuzzleColorButton puzzleColorButton)
        {
            text.DOColor(puzzleColorButton.buttonCode.color, durationexpected);
            text.text = interactionBind;
        }
        else if (obj is BasicButton)
        {
            text.text = interactionBind;
        }
        else if (obj is GraplingHookTarget)
        {
            IconImage? icon = GetIcon(icons, "GHOOK");
            if (icon is IconImage validicon)
                image.sprite = validicon.sprite;
        }

        Show(Constants.PanelNames.InteractionPopup, playerID);
    }

    // --- Cinematic bars ---
    private void TriggerCinematicBars(int playerID)
    {
        List<GameObject> cinematicPanels = GetPanel(playerID, Constants.PanelNames.GraplingHookCutscene);
        cinematicPanels.RemoveAll(go => go.name != "Top" && go.name != "Bottom");
        float halfDuration = Constants.Values.GraplingHookCutsceneDuration / 2f;
        AnimateCinematicBars(cinematicPanels, 250f, halfDuration);
    }

    private void AnimateCinematicBars(List<GameObject> panels, float size, float duration)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => ChangeRectSize(size, panels, duration));
        seq.AppendInterval(duration);
        seq.AppendCallback(() => ChangeRectSize(0f, panels, duration));
    }

    private void ChangeRectSize(float size, List<GameObject> panels, float duration)
    {
        foreach (GameObject panel in panels)
        {
            if (!panel.TryGetComponent(out RectTransform panelRect)) continue;
            float width = panelRect.rect.width;
            panelRect.DOSizeDelta(new Vector2(width, size), duration).SetEase(Ease.InOutCubic);
        }
    }

    private IconImage? GetIcon(List<IconImage> icons, string destiny)
    {
        foreach (IconImage icon in icons)
            if (icon.destiny == destiny) return icon;
        return null;
    }

    private List<GameObject> GetPanel(int playerID, string panel_name)
    {
        if (canvasMap.TryGetValue(playerID, out var dict) &&
            dict.TryGetValue(panel_name, out var result))
            return result;
        return new List<GameObject>();
    }
}

