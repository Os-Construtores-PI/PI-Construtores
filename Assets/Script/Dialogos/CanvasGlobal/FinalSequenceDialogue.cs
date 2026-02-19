using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FinalSequenceDialogue : MonoBehaviour
{
    [SerializeField] private DialogueTrigger finalDialogueTrigger;

    public void StartFinalSequence(DialogueTrigger trigger)
    {
        if (trigger == null) return;

        DialogueGlobal.Instance.SetTrigger(trigger);
        DialogueGlobal.Instance.IniciarDialogo(trigger._dialogo);
    }

    private IEnumerator FinalFlow()
    {
        GameDirector director = FindAnyObjectByType<GameDirector>();

        if(director != null && director.playerDirector != null)
        {
            director.SetLockPlayer(
                director.playerDirector.FirstPlayerContext,
                true );
        }

        DialogueGlobal.Instance.SetTrigger(finalDialogueTrigger);
        DialogueGlobal.Instance.IniciarDialogo(finalDialogueTrigger._dialogo);

        yield return new WaitUntil(() =>
        !DialogueGlobal.Instance._painelDialogo.activeSelf
        );

        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.Invoke();
    }
}
