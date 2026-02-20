using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FinalSequenceDialogue : MonoBehaviour
{
    [SerializeField] private DialogueTrigger finalDialogueTrigger;

    private bool isRunning = false;

    public void StartFinalSequence(DialogueTrigger trigger)
    {
        if (isRunning) return;
        if (trigger == null)
        {
            Debug.LogError("trigger passado é null");
            return;
        }

        StartCoroutine(FinalFlow(trigger));
    }

    private IEnumerator FinalFlow(DialogueTrigger trigger)
    {
        isRunning = true;

        GameDirector director = FindAnyObjectByType<GameDirector>();

        if (director != null && director.playerDirector != null)
        {
            director.SetLockPlayer(
                director.playerDirector.FirstPlayerContext,
                true);
        }

        DialogueGlobal.Instance.SetTrigger(trigger);
        DialogueGlobal.Instance.IniciarDialogo(trigger._dialogo);


        yield return new WaitUntil(() =>
           !DialogueGlobal.Instance._painelDialogo.activeSelf
        );

        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.Invoke();
    }
}
