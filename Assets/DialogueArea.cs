using System.Collections.Generic;
using UnityEngine;

public class DialogueArea : InteractableObject
{
    [TextArea(2, 6)]
    [SerializeField]
    private List<string> dialogues;

    [SerializeField]
    float dialogueSpeed = 20f;

    public override void Interaction(InfoPlayerInteraction info)
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDDIALOGUE.Invoke(
            info.PlayerContext,
            dialogues,
            dialogueSpeed
        );
    }
}
