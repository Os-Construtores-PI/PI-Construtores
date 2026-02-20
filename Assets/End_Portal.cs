using UnityEngine;

public class End_Portal : BasePortal
{
    [SerializeField] private DialogueTrigger finalDialogueTrigger;
    private bool actived = false;
    private void OnTriggerEnter(Collider other)
    {
        if (actived) return;
        if (!other.CompareTag("Player")) return;

        actived = true;

        FinalSequenceDialogue sequence = FindAnyObjectByType<FinalSequenceDialogue>();

        if(sequence != null)
        {
            sequence.StartFinalSequence(finalDialogueTrigger);
        }

       // gameObject.SetActive(false);

        //TriggerEndGame();
    }
  /*  private void TriggerEndGame()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.Invoke();
    }
  */
}
