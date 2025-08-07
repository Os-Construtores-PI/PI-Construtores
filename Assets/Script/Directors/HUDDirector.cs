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
    [SerializeField] private List<Painel> painelList = new();
    private Dictionary<string, List<GameObject>> painelMap;
    private TextMeshProUGUI interactionText;
    private Image interactionImage;

    private void Awake()
    {
        painelMap = new Dictionary<string, List<GameObject>>();
        foreach (var painel in painelList)
        {
            if (!painelMap.ContainsKey(painel.nome))
            {
                painelMap[painel.nome] = painel.painel;
            }
            else
            {
                Debug.LogWarning($"Painel duplicado: {painel.nome}");
            }
        }
        interactionText = painelMap[Constants.PanelNames.InteractionPopup][0].transform.GetComponentInChildren<TextMeshProUGUI>();
        interactionImage = painelMap[Constants.PanelNames.InteractionPopup][0].GetComponent<Image>();
    }

    private void Start()
    {
        DOTween.Init();
        GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
        GlobalEventBus.Instance.TriggeredCinematic.AddListener(TriggerCinematicBars);
        // Esconde os elementos do painel GameOver
        DisablePanel(Constants.PanelNames.GameOver, 0);
        DisablePanel(Constants.PanelNames.InteractionPopup, 0);
    }
    private void DisablePanel(string panel_name, float duration)
    {
        if (painelMap.TryGetValue(panel_name, out var panel))
        {
            int lenght = panel.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (i == 0 && panel[i].TryGetComponent(out Image image))
                {
                    image.DOFade(0f, duration);
                }
                panel[i].transform.localScale = Vector3.zero;
            }
        }
    }

    public void ShowFade(string panel_name)
    {
        if (painelMap.TryGetValue(panel_name, out var gameOverPainel))
        {
            int lenght = gameOverPainel.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (i == 0 && gameOverPainel[i].TryGetComponent(out Image imagem))
                {
                    imagem.DOFade(.8f, .25f);
                }
                gameOverPainel[i].transform.DOScale(Vector3.one, 0.25f);
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
    public void InteractionPopup(bool seeing, InteractableObject obj, int id)
    {
        float durationexpected = .25f;
        string interactionBind = InputSystem.actions.FindAction("Interaction").GetBindingDisplayString();
        if (seeing == false)
        {
            DisablePanel(Constants.PanelNames.InteractionPopup, durationexpected);
            interactionText.DOColor(Color.white, durationexpected);
            return;
        }
        if (obj is PuzzleColorButton puzzleColorButton)
        {
            interactionText.DOColor(puzzleColorButton.buttonCode.color, durationexpected);
            interactionText.text = interactionBind;
        }
        else if (obj is BasicButton)
        {
            interactionText.text = interactionBind;
        }
        else if (obj is GraplingHookTarget graplingHookTarget)
        {
            
        }
        ShowFade(Constants.PanelNames.InteractionPopup);
    }
    private void TriggerCinematicBars(int playerID)
    {
        List<GameObject> huds = GameObject.FindGameObjectsWithTag("HUD").ToList();
    }

}

