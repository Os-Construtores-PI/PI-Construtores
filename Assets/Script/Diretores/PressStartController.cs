using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PressStartController : MonoBehaviour
{
  [SerializeField]
  private LoadingScreen loadingScreen;

  [SerializeField]
  private GameObject pressStartPanel;

  [SerializeField]
  private Image _logoNekora;

  [SerializeField]
  private Image _logoBreadStudios;

  [SerializeField]
  private List<Image> _imagesToFadeIn = new();

  [SerializeField]
  private List<Image> _imagesToFadeOut = new();

  [SerializeField]
  private string menuScene = "MainMenu";

  private bool stated;

  void Start()
  {
    StartSequence();
  }

  private void StartSequence()
  {
    Sequence startSequence = DOTween.Sequence();
    _imagesToFadeIn.ForEach(image => startSequence.Join(image.DOFade(1, .5f)));
    startSequence.AppendInterval(.5f);
    startSequence.Append(_logoNekora.DOFade(1, .7f));
    startSequence.Append(_logoBreadStudios.DOFade(1, .7f));

    startSequence.AppendInterval(1f);
    _imagesToFadeOut.ForEach(image => startSequence.Join(image.DOFade(0, .5f)));
    startSequence.AppendCallback(() =>
    {
      pressStartPanel.SetActive(true);
    });

    startSequence.Play();
  }

  void Update()
  {
    if (!stated)
    {
      bool keyboard = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

      bool gamepad =
        Gamepad.current != null
        && (
          Gamepad.current.startButton.wasPressedThisFrame
          || Gamepad.current.startButton.wasPressedThisFrame
        );

      if (keyboard || gamepad)
      {
        stated = true;
        pressStartPanel.SetActive(false);
        loadingScreen.LoadScene(menuScene);
      }
    }
  }

  public void PressStartButton()
  {
    if (stated)
      return;

    stated = true;

    pressStartPanel.SetActive(false);

    loadingScreen.LoadScene(menuScene);
  }
}
