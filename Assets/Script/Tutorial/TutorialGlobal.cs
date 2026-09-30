using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class TutorialGlobal : MonoBehaviour
{
  public static TutorialGlobal Instance { get; private set; }


  [Header("Tutorial UI")]
  [SerializeField]
  private GameObject tutorialHUD;

  [SerializeField]
  private RectTransform interfaceLeft;

  [SerializeField]
  private RectTransform interfaceRight;

  [SerializeField]
  private GameObject videoPanel;


  [Header("Video")]
  [SerializeField]
  private VideoPlayer videoPlayer;

  [Header("Animation")]

  [SerializeField]
  private float interfaceStartX = -1200f;

  [SerializeField]
  private float interfaceDuration = 0.5f; 
}
