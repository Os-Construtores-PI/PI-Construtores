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
  private Transform _amethystContainer;

  [SerializeField]
  private Sprite _amethystSprite;

  [SerializeField]
  private GameObject _amethyst;
  private RectTransform _amethystTransform;

  //a
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
    _amethystTransform = _amethyst.GetComponent<RectTransform>();
    _amethystTransform.parent = _amethystContainer;
    _amethystTransform.localScale = Vector3.zero;
  }

  private void UpdateText(int newCount, Vector3? position = null)
  {
    if (_amethystText == null)
    {
      return;
    }

    if (position != null)
    {
      Sequence sequence = DOTween.Sequence();
      _amethystTransform.position = (Vector3)position;
      sequence.Append(_amethystTransform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), .5f));
      sequence.Append(_amethystTransform.DOScale(Vector3.one, .25f));
      sequence.Append(_amethystTransform.DOLocalMove(Vector3.zero, .5f));
      sequence.Append(_amethystTransform.DOScale(Vector3.zero, .25f));
      sequence.Play();
    }

    _amethystText.text = newCount.ToString("00");

    _amethystText.transform.DOKill();
    _amethystText.transform.localScale = Vector3.one;
    _amethystText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 1);
  }
}
