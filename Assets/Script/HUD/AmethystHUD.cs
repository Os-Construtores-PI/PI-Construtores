using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmethystHUD : MonoBehaviour
{
  [Header("Referência de UI")]
  [SerializeField]
  private TMP_Text _amethystText;

  [SerializeField]
  private Transform _amethystCounter;

  [SerializeField]
  private Sprite _amethystSprite;

  private GameObject _amethyst;
  private Transform _amethystTransform;
  void Start()
  {
    if (_amethystText == null)
      _amethystText = GetComponentInChildren<TMP_Text>();

    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateText);
    SetupAmethyst();
    UpdateText(0, null);
  }

  private void OnDestroy()
  {
    if (GlobalEventBus.HasInstance)
    {
      GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateText);
    }
  }

  private void SetupAmethyst()
  {
    _amethyst = new();
    _amethystTransform = _amethyst.transform;
    _amethystTransform.parent = _amethystCounter;
    Image tmpImage = _amethyst.AddComponent<Image>();
    tmpImage.sprite = _amethystSprite;
    _amethystTransform.localScale = Vector3.zero;
  }

  private void UpdateText(int newCount, Vector3? position = null)
  {
    if (_amethystText == null)
    {
      return;
    }

    if (position != null || position == default)
    {
      Sequence sequence = DOTween.Sequence();
      sequence.Append(_amethystTransform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), .5f));
      sequence.Append(_amethystTransform.DOScale(Vector3.one, .25f));
      sequence.Append(_amethystTransform.DOMove((Vector3)position, 1f));
      sequence.Append(_amethystTransform.DOMove(Vector3.zero, .5f));
      sequence.Append(_amethystTransform.DOScale(Vector3.zero, .25f));
    }

    _amethystText.text = newCount.ToString("00");

    _amethystText.transform.DOKill();
    _amethystText.transform.localScale = Vector3.one;
    _amethystText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 1);
  }
}
