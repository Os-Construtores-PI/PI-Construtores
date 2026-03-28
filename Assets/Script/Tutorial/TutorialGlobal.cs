using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialGlobal : MonoBehaviour
{
    public static TutorialGlobal Instance;

    [Header("Ui")]
    [SerializeField]
    private GameObject tutorialHUD;

    [Header("Tutoriais")]
    [SerializeField]
    private GameObject movimentoTutorial;

    [SerializeField]
    private GameObject dashTutorial;

    public event System.Action<bool> OnTutorialStateChanged;

    public bool IsTutorialActive { get; private set; }

    private PlayerInput _playerInput;

    private Tween currentTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //tutorialHUD.SetActive(false);
        DesativarTodos();
        Instance = this;
    }

    private void Start()
    {
        tutorialHUD.SetActive(false);
    }

    public void AbrirTutorial(TutorialTrigger.TutorialType tipo)
    {
        if (IsTutorialActive)
            return;

        IsTutorialActive = true;
        GameState.IsTutorialActive = true;

        DeviceSpriteManager.Instance?.ForceRefresh();

        DesativarTodos();
        //AtivarTutorial(tipo);

        tutorialHUD.SetActive(true);

        GameObject painel = GetPainel(tipo);
        if (painel != null)
            AnimarEntrada(painel);

        OnTutorialStateChanged?.Invoke(true);
    }

    public void FecharTutorial()
    {
        if (!IsTutorialActive)
            return;

        IsTutorialActive = false;
        GameState.IsTutorialActive = false;
        tutorialHUD.SetActive(false);
        GameObject painelAtivo = GetPainelAtivo();
        if (painelAtivo != null)
            AnimarSaida(painelAtivo);
        DeviceSpriteManager.Instance?.ForceRefresh();
        OnTutorialStateChanged?.Invoke(false);
    }

    private void AnimarEntrada(GameObject painel)
    {
        currentTween?.Kill();

        Time.timeScale = 0;
        painel.SetActive(true);

        CanvasGroup cg = painel.GetComponent<CanvasGroup>();
        RectTransform rt = painel.GetComponent<RectTransform>();

        cg.alpha = 0f;
        rt.localScale = Vector3.one * 0.9f;

        currentTween = DOTween
            .Sequence()
            .Append(cg.DOFade(1f, 0.25f))
            .Join(rt.DOScale(1f, 0.25f))
            .SetEase(Ease.OutBack)
            .SetUpdate(UpdateType.Normal, true);
    }

    private void AnimarSaida(GameObject painel)
    {
        currentTween?.Kill();

        CanvasGroup cg = painel.GetComponent<CanvasGroup>();
        RectTransform rt = painel.GetComponent<RectTransform>();
        Time.timeScale = 1;
        currentTween = DOTween
            .Sequence()
            .Append(cg.DOFade(0f, 0.2f))
            .Join(rt.DOScale(0.9f, 0.2f))
            .SetEase(Ease.InBack)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() =>
            {
                painel.SetActive(false);
                tutorialHUD.SetActive(false);
            });
    }

    private void DesativarTodos()
    {
        if (movimentoTutorial != null)
            movimentoTutorial.SetActive(false);
        if (dashTutorial != null)
            dashTutorial.SetActive(false);
    }

    private GameObject GetPainel(TutorialTrigger.TutorialType tipo)
    {
        return tipo switch
        {
            TutorialTrigger.TutorialType.Movimento => movimentoTutorial,
            TutorialTrigger.TutorialType.Dash => dashTutorial,
            _ => null,
        };
    }

    private GameObject GetPainelAtivo()
    {
        if (movimentoTutorial != null && movimentoTutorial.activeSelf)
            return movimentoTutorial;
        if (dashTutorial != null && dashTutorial.activeSelf)
            return dashTutorial;
        return null;
    }

    public static class GameState
    {
        public static bool IsTutorialActive;
        public static bool IsDialogueActive;
        public static bool IsPaused;

        public static bool CanPause()
        {
            if (IsTutorialActive)
                return false;
            if (IsDialogueActive)
                return false;
            return true;
        }
    }
}
