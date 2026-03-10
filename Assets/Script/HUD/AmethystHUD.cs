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

  private List<GameObject> _amethysts;
  private HashSet<GameObject> _inUse = new();

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

  private GameObject GetAvailable()
  {
    foreach (var obj in _amethysts)
    {
      if (!_inUse.Contains(obj))
        return obj;
    }
    return null; // todos em uso
  }

  private void UpdateText(int newCount, Vector3? position = null)
  {
    if (_amethystText == null)
      return;

    if (position != null)
    {
      // Guarda o count atual para não usar valor desatualizado depois
      int countSnapshot = newCount;
      VisualAmethyst((Vector3)position, countSnapshot);
    }
  }

  private void VisualAmethyst(Vector3 position, int newCount)
  {
    GameObject amethyst = GetAvailable();

    if (amethyst == null)
    {
      _amethystText.text = newCount.ToString("00");
      return;
    }

    _inUse.Add(amethyst);
    RectTransform rect = amethyst.GetComponent<RectTransform>();

    // Mata tweens anteriores SEM completar (evita callbacks velhos)
    rect.DOKill(complete: false);
    amethyst.SetActive(false);

    Vector3 worldTarget = _amethystContainer.position; // alvo em world space

    Sequence sequence = DOTween.Sequence().SetLink(amethyst);

    sequence.AppendCallback(() =>
    {
      amethyst.SetActive(true);
      rect.position = position + new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0f);
      rect.localScale = Vector3.zero;
      rect.localRotation = Quaternion.identity;
    });

    sequence.Append(
      rect.DOScale(new Vector3(1.5f, 1.5f, 1.5f), _scaleDuration).SetEase(_scaleEasing)
    );
    sequence.Join(rect.DOMove(worldTarget, _translationDuration).SetEase(_translationEasing));
    sequence.Join(rect.DORotate(new Vector3(0, 0, 10), _rotationDuration).SetEase(_rotationEasing));

    sequence.AppendCallback(() =>
    {
      _amethystText.transform.DOKill(complete: false);
      _amethystText.text = newCount.ToString("00");
      _amethystText.transform.localScale = Vector3.one;
      _amethystText
        .transform.DOPunchScale(Vector3.one * 2f, _scaleDuration, 1, 1)
        .SetLink(_amethystText.gameObject);
    });

    sequence.Append(rect.DOScale(Vector3.one, .25f));
    sequence.Append(rect.DOScale(Vector3.zero, .25f));

    sequence.OnComplete(() =>
    {
      amethyst.SetActive(false);
      _inUse.Remove(amethyst);
    });

    sequence.Play();
  }

  private void SetupAmethysts(List<GameObject> objects)
  {
    _amethysts = new List<GameObject>(objects);
    _inUse = new HashSet<GameObject>();
    UpdateText(0, transform.position);
  }
}
