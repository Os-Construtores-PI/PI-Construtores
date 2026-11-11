using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PandoraNavigationIndicator : MonoBehaviour
{
  public static PandoraNavigationIndicator Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private RectTransform _pandora;
    [SerializeField] private Image _pandoraImage;

    [Tooltip("Ponto que define a posição fixa da Pandora.")]
    [SerializeField] private RectTransform _positionReference;

    [Header("Configuração")]
    [SerializeField] private float _moveDuration = 0.25f;
    [SerializeField] private Ease _moveEase = Ease.OutCubic;

    [Header("Flutuação")]
    [SerializeField] private float _floatHeight = 8f;
    [SerializeField] private float _floatDuration = 0.8f;

    [Header("Piscar ao confirmar")]
    [SerializeField] private float _blinkDuration = 0.2f;
    [SerializeField] private int _blinkLoops = 6;

    private Tween _blinkTween;

    private Tween _floatTween;
    private float _currentY;

    private bool _isLocked;

    private void Awake()
    {
        Instance = this;

        if (_pandora == null)
            _pandora = GetComponent<RectTransform>();
    }

    public void MoveTo(float horizontalPosition)
    {

    if (_isLocked) return;


    if (_pandora == null || _positionReference == null)
      return;

    RectTransform parent = _pandora.parent as RectTransform;

    if (parent == null)
      return;

    Vector3 referenceLocal =
        parent.InverseTransformPoint(_positionReference.position);

    _floatTween?.Kill();
    _pandora.DOKill();

    _currentY = referenceLocal.y;

    _pandora.localPosition = new Vector3(
        _pandora.localPosition.x,
        _currentY,
        _pandora.localPosition.z
    );

    float destinationX = referenceLocal.x + horizontalPosition;

    _pandora.DOLocalMoveX(destinationX, _moveDuration)
        .SetEase(_moveEase)
        .SetUpdate(true)
        .OnComplete(StartFloating);
  }

  private void StartFloating()
  {
    if (_pandora == null)
      return;

    _floatTween?.Kill();

    _pandora.localPosition = new Vector3(
        _pandora.localPosition.x,
        _currentY,
        _pandora.localPosition.z
    );

    _floatTween = _pandora
        .DOLocalMoveY(_currentY + _floatHeight, _floatDuration)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo)
        .SetUpdate(true);
  }


    private void OnDestroy()
    {

        _floatTween?.Kill();
        _blinkTween?.Kill();


        if (_pandora != null)
            _pandora.DOKill();

        if (Instance == this)
            Instance = null;
    }

  public void BlinkOnConfirm()
  {
    if (_pandoraImage == null) return;

    _blinkTween?.Kill();

    Color color = _pandoraImage.color;
    color.a = 1f;
    _pandoraImage.color = color;

    _blinkTween = _pandoraImage
      .DOFade(0f, _blinkDuration)
      .SetLoops(_blinkLoops, LoopType.Yoyo)
      .SetUpdate(true)
      .OnComplete(() =>
      {
        if (_pandoraImage != null)
        {
          Color finalColor = _pandoraImage.color;
          finalColor.a = 1f;
          _pandoraImage.color = finalColor;
        }
      });
  }

  public void LockPosition()
  {
    _isLocked = true;

    _floatTween?.Kill();
    _floatTween = null;

    _pandora.DOKill();
  }
}
