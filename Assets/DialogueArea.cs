using System.Collections.Generic;
using UnityEngine;

public class DialogueArea : MonoBehaviour
{
    [TextArea(2,6)]
    [SerializeField]
    private List<string> dialogues;
    
    [SerializeField] float dialogueSpeed = 20f;

    private bool naturalTriggeredDialogue = false;

    private void OnTriggerEnter(Collider other)
    {
        if(naturalTriggeredDialogue) return;
        naturalTriggeredDialogue = true;
        if(other.TryGetComponent(out Player player))
        {
            GlobalEventBus.Instance.PLAYERTRIGGEREDDIALOGUE.Invoke(player.Context,dialogues,dialogueSpeed);
        }
    }
}
