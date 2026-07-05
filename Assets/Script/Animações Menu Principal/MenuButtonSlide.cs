using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuButtonSlide : MonoBehaviour
{

    [SerializeField] float _startDelay = .6f;
    [SerializeField] float _duration = .5f;
    [SerializeField] float _offSet = 350f;

    RectTransform rect;
    Vector2 finalPos;

    public static int ActiveAnimations;
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  void Awake()
  {
    rect = GetComponent<RectTransform>();
    finalPos = rect.anchoredPosition;
  }

  void OnEnable()
  {
    Play();
  }

  public void Play()
    {

      ActiveAnimations++;

    Debug.Log($"Come�ou {name} | {ActiveAnimations}");

    rect.DOKill();

        rect.anchoredPosition = finalPos + Vector2.right * _offSet;

        CanvasGroup cg = GetComponent<CanvasGroup>();

        cg.alpha = 0;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        Sequence seq = DOTween.Sequence();

        seq.SetDelay(_startDelay);

        seq.AppendCallback(() =>
        {
            cg.alpha = 1;
        });

        seq.Append(
            rect.DOAnchorPos(finalPos, _duration)
                .SetEase(Ease.OutExpo)
        );

        if(cg != null)
           seq.Join(
            cg.DOFade(1, _duration));
        

        seq.Join(rect.DOScale(1.08f, .12f)
                     .SetLoops(2, LoopType.Yoyo));

        seq.SetLink(gameObject);

    seq.OnComplete(() =>
{
    ActiveAnimations--;

  if(ActiveAnimations == 0)
  {
    MenuSelectable.CanSeletc = true;

    MenuSelectable[] buttons = 
        FindObjectsByType<MenuSelectable>(
          FindObjectsSortMode.None
        );

        foreach(var b in buttons)
          b.MostrarSprite();

    MenuDirector director = FindFirstObjectByType<MenuDirector>();

    director.EnableNavigation();
    director.ForceSelection();
  }

    
});

    }
}
