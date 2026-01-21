using DG.Tweening;
using UnityEngine;

public class ImageTriggerEvent : MonoBehaviour
{
    public CanvasGroup icon;
    public float _fadeDuration = 0.3f;

    public float _rotationSpeed = 90f; 

    private Tween _rotationTween;

    private bool playerInside = false;

    private bool dialogueBlocking = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    
    private void Awake()
    {
        icon.alpha = 0f;

        gameObject.SetActive(true);
       
    }

    private void OnEnable()
    {
        if (DialogueGlobal.Instance != null)
        {
            DialogueGlobal.Instance.OndialogueStart += OnDialogueStart;
            DialogueGlobal.Instance.OndialogueEnd += OnDialogueEnd;
        }
            
    }

    /*private void Instance_OndialogueStart()
    {
        throw new System.NotImplementedException();
    }
    */
    private void OnDisable()
    {
        if (DialogueGlobal.Instance != null)
        {
            DialogueGlobal.Instance.OndialogueStart -= OnDialogueStart;
            DialogueGlobal.Instance.OndialogueEnd -= OnDialogueEnd;
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        ShowIcon();
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        HideIconImediate();
    }
    public void StartSpin()
    {
        _rotationTween?.Kill();

        _rotationTween = transform
            .DORotate(new Vector3(0f, 180f, 0f),4f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void StopSpin()
    {
        _rotationTween?.Kill();
        _rotationTween = null;
    }

    public void HideIcon()
    {
        StopSpin();
        icon.DOKill();

        icon.DOFade(0f, _fadeDuration);
    }

    private void HideIconImediate()
    {
        StopSpin();
        icon.DOKill();
        icon.alpha = 0f;
    }

    public void ShowIcon()
    {
        if (!playerInside)
            return;

        icon.DOKill();
        icon.alpha = 0f;

        icon.DOFade(1f, _fadeDuration);
        StartSpin();
    }

    private void OnDialogueStart()
    {
        dialogueBlocking = true;
        HideIcon();
    }
    private void OnDialogueEnd()
    {
        dialogueBlocking = false;
        
        if (playerInside)
            ShowIcon();
    }


}
