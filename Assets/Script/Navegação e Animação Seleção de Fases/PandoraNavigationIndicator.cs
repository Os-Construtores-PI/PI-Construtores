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

    private void Awake()
    {
        Instance = this;

        if (_pandora == null)
            _pandora = GetComponent<RectTransform>();
    }

    public void MoveTo(float horizontalPosition)
    {
        if (_pandora == null || _positionReference == null) return;

        RectTransform parent = _pandora.parent as RectTransform;

        if (parent == null) return;

        Vector3 referenceLocal = 
               parent.InverseTransformPoint(_positionReference.position);

        Vector3 currentPosition = _pandora.localPosition;

        _pandora.localPosition = new Vector3(
          currentPosition.x,
          referenceLocal.y,
          currentPosition.z );

        float destinationX = referenceLocal.x + horizontalPosition;

        _pandora.DOKill();

         _pandora.DOLocalMoveX(destinationX, _moveDuration)
                 .SetEase(_moveEase)
                 .SetUpdate(true);
    }


    private void OnDestroy()
    {
        if (_pandora != null)
            _pandora.DOKill();

        if (Instance == this)
            Instance = null;
    }
}
