using UnityEngine;

public class DialogueAutoActivator : MonoBehaviour
{
  [Header("Configurações")]
  public TutorialTrigger targetTrigger;
  public bool onlyOnce = true; // Executa só uma vez
  private bool alreadyTriggered = false;

  private void OnTriggerEnter(Collider other)
  {
    Player player = other.GetComponentInParent<Player>();

    if (player == null)
        return;
    
    if (onlyOnce && alreadyTriggered)
       return;
    
    if (targetTrigger == null)
    {
      Debug.LogWarning(
        "[DialogueAutoActivator] nenhum tutorial atribuido",
        this
      );

      return;
    }

    if (onlyOnce)
        alreadyTriggered = true;
    
    targetTrigger.OpenTutorialAutomatically(player);
  }
}
