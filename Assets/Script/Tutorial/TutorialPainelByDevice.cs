using UnityEngine;
using UnityEngine.UI;

public class TutorialPainelByDevice : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image tutorialImage;

    [Header("Painels")]
    [SerializeField] private Sprite keyboardPainel;
    [SerializeField] private Sprite xboxPainel;
    [SerializeField] private Sprite playstationPainel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        if (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged += Atualizar;

        if (TutorialGlobal.Instance != null)
            TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

        Atualizar(DeviceSpriteManager.Instance.GetCurrentDevice());
    }

    private void OnDisable()
    {
        if  (DeviceSpriteManager.Instance != null)
            DeviceSpriteManager.Instance.OnDeviceChanged -= Atualizar;

        if(TutorialGlobal.Instance != null)
            TutorialGlobal.Instance.OnTutorialStateChanged -= OnTutorialStateChanged;
    }

    private void OnTutorialStateChanged(bool ativo)
    {
        if (!ativo)
        {
            if(tutorialImage != null)
                tutorialImage.enabled = false;
            return;
        }
        if(tutorialImage != null)
        {
            tutorialImage.enabled = true;
            Atualizar(DeviceSpriteManager.Instance.GetCurrentDevice());
        }

    }

    private void Atualizar(string device)
    {
        if (TutorialGlobal.Instance == null) return;
        if (!TutorialGlobal.Instance.IsTutorialActive) return;
        if (tutorialImage == null) return;

        tutorialImage.sprite = device switch
        {
            "Keyboard" => keyboardPainel,
            "Xbox" => xboxPainel,
            "Playstation" => playstationPainel,
            _ => tutorialImage.sprite
        };

        
    }

    
}
