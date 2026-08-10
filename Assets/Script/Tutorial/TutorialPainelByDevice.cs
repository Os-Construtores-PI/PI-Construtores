using UnityEngine;
using UnityEngine.UI;

public class TutorialPainelByDevice : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private Image tutorialImage;

  private void OnEnable()
  {
    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

    DeviceInputManager.Instance.ForceRefresh();
  }

  private void OnDisable()
  {
    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged -= OnTutorialStateChanged;
  }

  private void OnTutorialStateChanged(bool ativo)
  {
    if (!ativo)
    {
      if (tutorialImage != null)
        tutorialImage.enabled = false;
      return;
    }
    if (tutorialImage != null)
    {
      tutorialImage.enabled = true;
      DeviceInputManager.Instance.ForceRefresh();
    }
  }
}
