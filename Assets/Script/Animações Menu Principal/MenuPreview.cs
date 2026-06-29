using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuPreview : MonoBehaviour
{
    public static MenuPreview Instance;

    [SerializeField] private Image _previewImagem;

    [Header("Positions")]
    [SerializeField] private RectTransform rect;

    [SerializeField] private Vector2 _finalPosition;
    [SerializeField] private float _entranceOffSet = 900f;

    [SerializeField] private float duration = .45f;

    private Tween currentTween;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    Instance = this;
  }

  public void Show(PreviewSettings preview)
    {
        if(preview == null)
           return;
        
        currentTween?.Kill();

        _previewImagem.sprite = preview.sprite;

        rect.sizeDelta = preview.size;

        rect.localScale = Vector3.one * preview.scale;

        rect.anchoredPosition =
            preview.position + Vector2.right * _entranceOffSet;
        
        _previewImagem.color = new Color(1,1,1,0);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            rect.DOAnchorPos(preview.position, duration)
                .SetEase(Ease.OutExpo)
        );

        seq.Join(
            _previewImagem.DOFade(1,duration)
        );

        currentTween = seq;
    }


}
