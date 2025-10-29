using DG.Tweening;
using UnityEngine;

public class BaseRenderedGameObject : MonoBehaviour
{
    private readonly float pulseDuration = 0.4f;
    private readonly float scaleFactor = 1.3f;
    private bool hasPlayed;
    protected Vector3 initialScale;
    private Tween scaleTween;
    protected bool canPulse = true;

    public virtual void Awake()
    {
        initialScale = transform.localScale;
    }

    public virtual void Start()
    {
        DOTween.Init(); 
    }

    void OnBecameVisible()
    {
        if ((!gameObject.activeInHierarchy || hasPlayed) && canPulse) return;
        hasPlayed = true;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(initialScale * scaleFactor, pulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutBack)
            .OnComplete(() => scaleTween = null);
    }


    void OnBecameInvisible()
    {
        hasPlayed = false;
    }

    void OnDisable() => DOTween.Kill(transform);
}
