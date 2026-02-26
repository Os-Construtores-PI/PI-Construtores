using UnityEngine;

public class End_Portal : BasePortal
{
    [SerializeField] private FinalSequenceDialogue finalSequence;
    [SerializeField] private DialogueTrigger enemyFinalTrigger;
    
    private bool actived = false;
    private void OnTriggerEnter(Collider other)
    {
        if (actived) return;
        if (!other.CompareTag("Player")) return;

        actived = true;

        FinalSequenceDialogue sequence = FindAnyObjectByType<FinalSequenceDialogue>();

        if(sequence != null)
        {
            sequence.StartFinalSequence(enemyFinalTrigger);
        }
        else
        {
          Debug.LogError("FinalSequenceDialogue não atribuído no Inspector!");
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
