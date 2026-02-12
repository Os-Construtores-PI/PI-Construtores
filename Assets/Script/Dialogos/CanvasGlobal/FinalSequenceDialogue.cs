using DG.Tweening;
using UnityEngine;

public class FinalSequenceDialogue : MonoBehaviour
{
    public static FinalSequenceDialogue Instance;

    [Header("Dialogo Final")]
    [SerializeField] private DialogueTrigger finalDialogue;

    [Header("Painel Final")]
    [SerializeField] private GameObject painelFinalMenu;

    private bool _finalStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        painelFinalMenu.SetActive(false);
    }

    private void OnEnable()
    {
        if (DialogueGlobal.Instance != null)
            DialogueGlobal.Instance.OndialogueEnd += OnFinalDialogueEnd;
    }

    private void OnDisable()
    {
        if(DialogueGlobal.Instance != null)
            DialogueGlobal.Instance.OndialogueEnd -= OnFinalDialogueEnd;
    }

    // Update is called once per frame
    

    /// <summary>
    /// Chamado quando a fase termina
    /// </summary>
    /// 
    public void StartFinalSequence()
    {
        if (_finalStarted) return;
        _finalStarted = true;

        if(finalDialogue == null)
        {
            Debug.LogError("[FinalSequence] DialogueTrigger final não atribuido!");
            return;
        }
        DialogueGlobal.Instance.SetTrigger(finalDialogue);
        DialogueGlobal.Instance.IniciarDialogo(finalDialogue._dialogo);

    }

    private void OnFinalDialogueEnd()
    {
        if (!_finalStarted) return;

        AbrirPainelFinal();
    }

    private void AbrirPainelFinal()
    {
        painelFinalMenu.SetActive(true);

        Transform t = painelFinalMenu.transform;
        t.localScale = Vector3.zero;

        t.DOScale(1f, 0.4f)
         .SetEase(Ease.OutBack)
         .SetUpdate(true);
    }
}
