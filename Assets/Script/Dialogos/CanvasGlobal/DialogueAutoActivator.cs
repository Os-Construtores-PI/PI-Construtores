using UnityEngine;

public class DialogueAutoActivator : MonoBehaviour
{
    [Header("Configurações")]
    public TutorialTrigger targetTrigger;
    public bool onlyOnce = true; // Executa só uma vez
    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (onlyOnce && alreadyTriggered)
            return;

        if (targetTrigger != null)
        {
            targetTrigger.AbriirTutorial();
            alreadyTriggered = true;
        }
        else
        {
            Debug.LogWarning("[DialogueAutoActivator] Nenhum DialogueTrigger atribuído!", this);
        }
    }
}
