using DG.Tweening;
using TMPro;
using UnityEngine;

public class NumeroAmetistasColetadas : MonoBehaviour
{
    [Header("Referência de UI")]
    [SerializeField] private TMP_Text _amethystText;
    void Start()
    {
        if (_amethystText == null)
            _amethystText = GetComponentInChildren<TMP_Text>();

        UpdateText(CollectibleManager.Instance.GetCurrentColletables());

        CollectibleManager.Instance.OnColletableCountChanged += UpdateText;

    }

    private void OnDestroy()
    {
        if (CollectibleManager.Instance != null)
            CollectibleManager.Instance.OnColletableCountChanged -= UpdateText;
    }

    private void UpdateText(int newCount)
    {
        if (_amethystText == null) return;

        _amethystText.text = newCount.ToString("00");

        _amethystText.transform.DOKill();
        _amethystText.transform.localScale = Vector3.one;
        _amethystText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 1);
    }
}
