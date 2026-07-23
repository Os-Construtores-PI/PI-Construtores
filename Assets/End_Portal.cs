using UnityEngine;

public class End_Portal : BasePortal
{
  [SerializeField]
  private FinalSequenceDialogue finalSequence;

  [SerializeField]
  private DialogueTrigger enemyFinalTrigger;

  private bool actived = false;

  public void OnTriggerEnter(Collider other)
  {
    if (actived)
      return;
    if (!other.CompareTag("Player"))
      return;

    actived = true;

    // FinalSequenceDialogue sequence = FindAnyObjectByType<FinalSequenceDialogue>();

    // if (sequence != null)
    // {
    //   sequence.StartFinalSequence(enemyFinalTrigger);
    // }
    // else
    // {
    //   Debug.LogError("[End_Portal] FinalSequenceDialogue não atribuído no Inspector!");
    // }

    TriggerEndGame();
  }

  private void TriggerEndGame()
  {
    GlobalEventBus.Instance.EndGame.Invoke();
  }
}
