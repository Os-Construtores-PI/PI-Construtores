using System.Collections.Generic;
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

    void Start()
    {
        DOTween.Init(logBehaviour: LogBehaviour.Verbose, recycleAllByDefault: true);

        if (_amethystText == null)
            _amethystText = GetComponentInChildren<TMP_Text>();

        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.AddListener(UpdateText);
        _amethystSpawner.FinishedInstancing.AddListener(SetupAmethysts);
    }

    private void OnDestroy()
    {
        if (GlobalEventBus.HasInstance)
            GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.RemoveListener(UpdateText);

        // Libera todos os objetos em uso ao destruir o HUD
        ForceReleaseAll();
    }

    private void OnDisable()
    {
        // Garante liberação também quando o objeto é desativado
        ForceReleaseAll();
    }

    private void ForceReleaseAll()
    {
        if (_amethysts == null)
            return;

        foreach (var obj in _amethysts)
        {
            if (obj == null)
                continue;
            obj.GetComponent<RectTransform>()?.DOKill(complete: false);
            obj.SetActive(false);
        }

        _inUse.Clear();
    }

    private GameObject GetAvailable()
    {
        if (_amethysts == null)
            return null;

        foreach (var obj in _amethysts)
        {
            if (!_inUse.Contains(obj))
                return obj;
        }
        return null;
    }

    private void UpdateText(int newCount, Vector3? position = null)
    {
        if (_amethystText == null)
            return;

        if (position != null)
        {
            VisualAmethyst((Vector3)position, newCount);
            return;
        }

        _amethystText.text = newCount.ToString("00");
    }

    private void VisualAmethyst(Vector3 position, int newCount)
    {
        GameObject amethyst = GetAvailable();

        if (amethyst == null)
            return; // Texto já foi atualizado no UpdateText, não precisa fazer nada aqui

        _inUse.Add(amethyst);

        RectTransform rect = amethyst.GetComponent<RectTransform>();

        amethyst.SetActive(false);
        rect.DOKill(complete: false);

        Vector3 worldTarget = _amethystContainer.position;

        // FIX: SetUpdate(true) em toda a Sequence para ignorar Time.timeScale (pausa do jogo)
        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(rect);

        sequence.AppendCallback(() =>
        {
            amethyst.SetActive(true);
            rect.position =
                position + new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0f);
            rect.localScale = Vector3.zero;
            rect.localRotation = Quaternion.identity;
        });

        // FIX: SetUpdate(true) em cada tween filho também
        sequence.Append(
            rect.DOScale(new Vector3(1.5f, 1.5f, 1.5f), _scaleDuration)
                .SetEase(_scaleEasing)
                .SetUpdate(true)
        );

        sequence.Join(
            rect.DOMove(worldTarget, _translationDuration)
                .SetEase(_translationEasing)
                .SetUpdate(true)
        );

        sequence.Join(
            rect.DORotate(new Vector3(0, 0, 10), _rotationDuration)
                .SetEase(_rotationEasing)
                .SetUpdate(true)
        );

        // FIX: Só faz o punch agora, o texto já foi atualizado no UpdateText
        sequence.AppendCallback(() =>
        {
            _amethystText.transform.DOKill(complete: false);
            _amethystText.transform.localScale = Vector3.one;
            _amethystText
                .transform.DOPunchScale(Vector3.one * 2f, _scaleDuration, 1, 1)
                .SetUpdate(true)
                .SetLink(_amethystText.gameObject);
            _amethystText.text = newCount.ToString("00");
        });

        sequence.Append(rect.DOScale(Vector3.one, .25f).SetUpdate(true));

        sequence.Append(rect.DOScale(Vector3.zero, .25f).SetUpdate(true));

        // FIX: OnComplete para o caminho feliz
        sequence.OnComplete(() =>
        {
            amethyst.SetActive(false);
            _inUse.Remove(amethyst);
        });

        // FIX: OnKill garante liberação se a sequência for interrompida por qualquer motivo
        // (pausa, cena recarregada, objeto destruído, DOKill externo, etc.)
        sequence.OnKill(() =>
        {
            if (_inUse.Contains(amethyst))
            {
                amethyst.SetActive(false);
                _inUse.Remove(amethyst);
            }
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
