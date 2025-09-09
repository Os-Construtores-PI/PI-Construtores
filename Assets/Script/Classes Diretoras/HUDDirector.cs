using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDDirector : MonoBehaviour
{
    [SerializeField] private List<CustomCanvas> canvas = new();
    [SerializeField] private List<IconImage> icons = new();

    private Dictionary<int, Dictionary<string, List<GameObject>>> canvasMap;
    private Dictionary<int, TextMeshProUGUI> interactionTexts = new();
    private Dictionary<int, Image> interactionImages = new();
    private Dictionary<int, Sprite> ogSprites = new();

    private void Awake()
    {
        canvasMap = new();

        foreach (CustomCanvas c in canvas)
        {
            int id = c.playerID;

            if (!canvasMap.ContainsKey(id))
                canvasMap[id] = new();

            foreach (CustomPanel panel in c.panels)
            {
                if (!canvasMap[id].ContainsKey(panel.nome))
                    canvasMap[id][panel.nome] = new();

                canvasMap[id][panel.nome].AddRange(panel.painel);
            }
        }

        // Inicializa textos e imagens de interação por player
        foreach (var kvp in canvasMap)
        {
            int playerID = kvp.Key;

            if (canvasMap[playerID].TryGetValue(Constants.PanelNames.InteractionPopup, out var panels) && panels.Count > 0)
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
        }
    }

    private void Start()
    {
        DOTween.Init();
        GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
        GlobalEventBus.Instance.TriggeredCinematic.AddListener(TriggerCinematicBars);

        foreach (int playerID in canvasMap.Keys)
        {
            DisablePanelWithImage(Constants.PanelNames.GameOver, 0, playerID);
            DisablePanel(Constants.PanelNames.InteractionPopup, 0, playerID);
        }
    }

    private void DisablePanelWithImage(string panel_name, float duration, int playerID)
    {
        if (canvasMap.TryGetValue(playerID, out var panelMap) &&
            panelMap.TryGetValue(panel_name, out var panel))
        {
            foreach (var go in panel)
            {
                if (go.TryGetComponent(out Image image))
                {
                    image.DOFade(0f, duration);
                }
                go.transform.DOScale(Vector3.zero, 0.25f);
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
                go.transform.DOScale(Vector3.zero, 0.25f);
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
                {
                    image.DOFade(.8f, .25f);
                }
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
            {
                go.transform.DOScale(Vector3.one, 0.25f);
            }
        }
    }

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
        yield return new WaitForSecondsRealtime(.25f);
        noise.AmplitudeGain = 0;
    }

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
            {
                image.sprite = validicon.sprite;
            }
        }

        Show(Constants.PanelNames.InteractionPopup, playerID);
    }

    private void TriggerCinematicBars(int playerID)
    {
        List<GameObject> cinematicPanels = GetPanel(playerID, Constants.PanelNames.GraplingHookCutscene);
        cinematicPanels.RemoveAll(go => go.name!="Top" && go.name!="Bottom");
        float halfDuration = Constants.Values.GraplingHookCutsceneDuration / 2f;

        // Altura desejada das barras
        float barHeight = 250f; 

        // Chama a sequência
        AnimateCinematicBars(cinematicPanels, barHeight, halfDuration);
    }

    private void AnimateCinematicBars(List<GameObject> panels, float size, float duration)
    {
        Sequence seq = DOTween.Sequence();

        // Aumentar (entrada das barras)
        seq.AppendCallback(() => ChangeRectSize(size, panels, duration));

        // Espera com barras visíveis
        seq.AppendInterval(duration);

        // Diminuir (saída das barras)
        seq.AppendCallback(() => ChangeRectSize(0f, panels, duration));
    }

    private void ChangeRectSize(float size, List<GameObject> panels, float duration)
    {
        foreach (GameObject panel in panels)
        {
            if (!panel.TryGetComponent(out RectTransform panelRect)) continue;
            float width = panelRect.rect.width; // largura real do painel no Canvas
            panelRect.DOSizeDelta(new Vector2(width, size), duration).SetEase(Ease.InOutCubic);
        }
    }

    private IconImage? GetIcon(List<IconImage> icons, string destiny)
    {
        foreach (IconImage icon in icons)
        {
            if (icon.destiny == destiny)
            {
                return icon;
            }
        }
        return null;
    }
    private List<GameObject> GetPanel(int playerID, string panel_name)
    {
        if (canvasMap.TryGetValue(playerID, out var dict) &&
            dict.TryGetValue(panel_name, out var result))
        {
            return result;
        }
        return null;
    }
}
