using System.Collections.Generic;
using System.Threading.Tasks;
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
  private ManualSingleSpawner _amethystSpawner;

  private Stack<GameObject> _amethysts;

  [Header("Config")]
  [SerializeField]
  private float _translationDuration = .7f;

  [SerializeField]
  private float _rotationDuration = .5f;

  [SerializeField]
  private float _scaleDuration = .5f;

  [SerializeField]
  private Ease _scaleEasing = Ease.InSine;

  [SerializeField]
  private Ease _rotationEasing = Ease.InSine;

  [SerializeField]
  private Ease _translationEasing = Ease.InSine;

  //a
  void Start()
  {
    DOTween.Init();
    if (_amethystText == null)
      _amethystText = GetComponentInChildren<TMP_Text>();

    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateText);
    _amethystSpawner.FinishedInstancing.AddListener(SetupAmethysts);
  }

  private void OnDestroy()
  {
    if (GlobalEventBus.HasInstance)
    {
      GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateText);
    }
  }

  private void SetupAmethysts(List<GameObject> objects)
  {
    _amethysts = new Stack<GameObject>(objects);
    UpdateText(0, transform.position);
  }

  private void UpdateText(int newCount, Vector3? position = null)
  {
    if (_amethystText == null)
    {
      return;
    }

    if (position != null)
    {
      VisualAmethyst((Vector3)position, newCount);
    }
  }

  private void VisualAmethyst(Vector3 position, int newCount)
  {
    GameObject amethyst = _amethysts.Pop();
    RectTransform rect = amethyst.GetComponent<RectTransform>();

    Sequence sequence = DOTween.Sequence();

    sequence.AppendCallback(() =>
    {
      amethyst.SetActive(true);
      rect.position = position + new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f));
      rect.localScale = Vector3.zero;
      rect.localRotation = Quaternion.identity;
    });

    sequence.Append(
      rect.DOScale(new Vector3(1.5f, 1.5f, 1.5f), _scaleDuration).SetEase(_scaleEasing)
    );
    sequence.Join(rect.DOLocalMove(Vector3.zero, _translationDuration).SetEase(_translationEasing));
    sequence.Join(rect.DORotate(new Vector3(0, 0, 10), _rotationDuration).SetEase(_rotationEasing));
    sequence.AppendCallback(() =>
    {
      _amethystText.transform.DOKill();
      _amethystText.text = newCount.ToString("00");
      _amethystText.transform.localScale = Vector3.one;
      _amethystText.transform.DOPunchScale(Vector3.one * 2f, _scaleDuration, 1, 1);
    });

    sequence.Append(rect.DOScale(Vector3.one, .25f));
    sequence.Append(rect.DOScale(Vector3.zero, .25f));

    // Callback de Finalização: Limpa e devolve para a stack
    sequence.OnComplete(() =>
    {
      amethyst.SetActive(false);
      _amethysts.Push(amethyst);
    });

    sequence.Play();
  }
}
